#region Copyright (C) 2007-2011 Team MediaPortal

/*
    Copyright (C) 2007-2011 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Core.MediaManagement.ResourceAccess;
using MediaPortal.UI.Players.Video.Interfaces;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.Core.PluginManager;
using MediaPortal.Core.Services.PluginManager.Builders;

namespace MediaPortal.UI.Players.Video
{
  /// <summary>
  /// Plugin item builder for <c>VideoPlayerMimeTypeMapping</c> plugin items.
  /// </summary>
  public class VideoPlayerMimeTypeMappingBuilder : IPluginItemBuilder
  {
    #region IPluginItemBuilder Member

    public object BuildItem(PluginItemMetadata itemData, PluginRuntime plugin)
    {
      BuilderHelper.CheckParameter("ClassName", itemData);
      BuilderHelper.CheckParameter("MimeType", itemData);
      return new VideoPlayerMimeTypeMapping(plugin.GetPluginType(itemData.Attributes["ClassName"]), itemData.Attributes["MimeType"]);
    }

    public void RevokeItem(object item, PluginItemMetadata itemData, PluginRuntime plugin)
    {

    }

    public bool NeedsPluginActive(PluginItemMetadata itemData, PluginRuntime plugin)
    {
      return true;
    }

    #endregion
  }

  public class VideoPlayerMimeTypeMapping
  {
    public Type PlayerClass { get; private set; }
    public string MimeType { get; private set; }

    public VideoPlayerMimeTypeMapping(Type type, string mimetype)
    {
      PlayerClass = type;
      MimeType = mimetype;
    }
  }

  /// <summary>
  /// Player builder for all video players of the VideoPlayers plugin.
  /// </summary>
  public class VideoPlayerBuilder : IPlayerBuilder
  {
    #region Classes

    protected class VideoPlayerBuilderPluginItemStateTracker : IPluginItemStateTracker
    {
      public string UsageDescription
      {
        get { return "VideoPlayerBuilder - MimeType registration"; }
      }

      public bool RequestEnd(PluginItemRegistration itemRegistration)
      {
        return false;
      }

      public void Stop(PluginItemRegistration itemRegistration)
      {
        // Nothing to do
      }

      public void Continue(PluginItemRegistration itemRegistration)
      {
        // Nothing to do
      }
    }

    #endregion

    #region Consts

    /// <summary>
    /// Path where mappings of mimetypes to player types are registered.
    /// </summary>
    public const string VIDEOPLAYERBUILDERMIMETYPES_REGISTRATION_PATH = "/VideoPlayers/MimeTypeRegistrations";

    #endregion

    #region Protected fields

    protected VideoPlayerBuilderPluginItemStateTracker _videoPlayerBuilderPluginItemStateTracker;

    #endregion

    #region Ctor
    
    public VideoPlayerBuilder()
    {
      _videoPlayerBuilderPluginItemStateTracker = new VideoPlayerBuilderPluginItemStateTracker();

      IPluginManager pluginManager = ServiceRegistration.Get<IPluginManager>();
      foreach (PluginItemMetadata itemMetadata in pluginManager.GetAllPluginItemMetadata(VIDEOPLAYERBUILDERMIMETYPES_REGISTRATION_PATH))
      {
        VideoPlayerMimeTypeMapping playerMapping = pluginManager.RequestPluginItem<VideoPlayerMimeTypeMapping>(
            VIDEOPLAYERBUILDERMIMETYPES_REGISTRATION_PATH, itemMetadata.Id, _videoPlayerBuilderPluginItemStateTracker);
        if (playerMapping == null)
          ServiceRegistration.Get<ILogger>().Warn("Could not instantiate VideoPlayerMimeTypeMapping with id '{0}'", itemMetadata.Id);
        else
          PlayerRegistration.AddMimeTypeMapping(playerMapping.MimeType, playerMapping.PlayerClass);
      }
    }
    
    #endregion

    #region IPlayerBuilder implementation

    public IPlayer GetPlayer(IResourceLocator locator, string mimeType)
    {
      Type playerType = PlayerRegistration.GetPlayerTypeForMediaItem(locator, mimeType);
      if (playerType == null)
        return null;
      IInitializablePlayer player = (IInitializablePlayer) Activator.CreateInstance(playerType);
      try
      {
        player.SetMediaItemLocator(locator);
      }
      catch (Exception e)
      { // The file might be broken, so the player wasn't able to play it
        ServiceRegistration.Get<ILogger>().Warn("{0}: Unable to play '{1}'", e, playerType, locator);
        if (player is IDisposable)
          ((IDisposable) player).Dispose();
        player = null;
      }
      return (IPlayer) player;
    }

    #endregion
  }
}