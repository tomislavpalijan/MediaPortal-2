#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Drawing;
using MediaPortal.UI.SkinEngine.Rendering;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public class ScrollContentPresenter : ContentPresenter, IScrollInfo, IScrollViewerFocusSupport
  {
    #region Consts

    public const int NUM_SCROLL_PIXEL = 50;

    #endregion

    #region Protected fields

    protected bool _canScroll = false;
    protected float _scrollOffsetX = 0;
    protected float _scrollOffsetY = 0;
    protected float _actualScrollOffsetX = 0;
    protected float _actualScrollOffsetY = 0;

    #endregion

    #region Ctor

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      ScrollContentPresenter scp = (ScrollContentPresenter) source;
      _canScroll = scp._canScroll;
      _scrollOffsetX = 0;
      _scrollOffsetY = 0;
    }

    #endregion

    private void InvokeScrolled()
    {
      ScrolledDlgt dlgt = Scrolled;
      if (dlgt != null) dlgt(this);
    }

    public void SetScrollOffset(float scrollOffsetX, float scrollOffsetY)
    {
      if (_scrollOffsetX == scrollOffsetX && _scrollOffsetY == scrollOffsetY)
        return;
      if (scrollOffsetX < ActualWidth - TotalWidth)
        scrollOffsetX = (float) ActualWidth - TotalWidth;
      if (scrollOffsetY < ActualHeight - TotalHeight)
        scrollOffsetY = (float) ActualHeight - TotalHeight;
      if (scrollOffsetX > 0)
        scrollOffsetX = 0;
      if (scrollOffsetY > 0)
        scrollOffsetY = 0;
      _scrollOffsetX = scrollOffsetX;
      _scrollOffsetY = scrollOffsetY;
      InvalidateLayout();
      InvokeScrolled();
    }

    public override void MakeVisible(UIElement element, RectangleF elementBounds)
    {
      if (_canScroll)
      {
        float differenceX = 0;
        float differenceY = 0;
        if (elementBounds.X + elementBounds.Width > ActualPosition.X + ActualWidth)
          differenceX = - (float) (elementBounds.X + elementBounds.Width - ActualPosition.X - ActualWidth);
        if (elementBounds.X + differenceX < ActualPosition.X)
          differenceX = ActualPosition.X - elementBounds.X;
        if (elementBounds.Y + elementBounds.Height > ActualPosition.Y + ActualHeight)
          differenceY = - (float) (elementBounds.Y + elementBounds.Height - ActualPosition.Y - ActualHeight);
        if (elementBounds.Y + differenceY < ActualPosition.Y)
          differenceY = ActualPosition.Y - elementBounds.Y;

        // Change rect as if children were already re-arranged
        elementBounds.X += differenceX;
        elementBounds.Y += differenceY;
        SetScrollOffset(_actualScrollOffsetX + differenceX, _actualScrollOffsetY + differenceY);
      }
      base.MakeVisible(element, elementBounds);
    }

    protected override void ArrangeTemplateControl()
    {
      if (_templateControl == null)
      {
        _scrollOffsetX = 0;
        _scrollOffsetY = 0;
      }
      else
      {
        SizeF desiredSize = _templateControl.DesiredSize;
        PointF position;
        SizeF availableSize;
        if (_canScroll)
        {
          availableSize = _innerRect.Size;
          if (desiredSize.Width > _innerRect.Width)
          {
            _scrollOffsetX = Math.Max(_scrollOffsetX, _innerRect.Width - desiredSize.Width);
            availableSize.Width = desiredSize.Width;
          }
          else
            _scrollOffsetX = 0;
          if (desiredSize.Height > _innerRect.Height)
          {
            _scrollOffsetY = Math.Max(_scrollOffsetY, _innerRect.Height - desiredSize.Height);
            availableSize.Height = desiredSize.Height;
          }
          else
            _scrollOffsetY = 0;
          position = new PointF(_innerRect.X + _scrollOffsetX, _innerRect.Y + _scrollOffsetY);
        }
        else
        {
          _scrollOffsetX = 0;
          _scrollOffsetY = 0;
          position = new PointF(_innerRect.X, _innerRect.Y);
          availableSize = _innerRect.Size;
        }

        ArrangeChild(_templateControl, _templateControl.HorizontalAlignment, _templateControl.VerticalAlignment,
            ref position, ref availableSize);
        RectangleF childRect = new RectangleF(position, availableSize);
        _templateControl.Arrange(childRect);
      }
      _actualScrollOffsetX = _scrollOffsetX;
      _actualScrollOffsetY = _scrollOffsetY;
    }

    public override bool IsChildRenderedAt(UIElement child, float x, float y)
    {
      // The ScrollContentPresenter clips all rendering outside its range, so first check if x and y are in its area
      return IsInArea(x, y) && base.IsChildRenderedAt(child, x, y);
    }

    public override void DoRender(RenderContext localRenderContext)
    {
      base.DoRender(localRenderContext); // Do the actual rendering
      // The following line cuts all child output outside our bounds. The style for ScrollContentPresenter must
      // use an opacity brush to avoid drawing children outside our range.
      localRenderContext.SetUntransformedBounds(ActualBounds);
    }

    #region IScrollViewerFocusSupport implementation

    public bool FocusUp()
    {
      if (!MoveFocus1(MoveFocusDirection.Up))
        // We couldn't move the focus - fallback: move physical scrolling offset
        if (IsViewPortAtTop)
          return false;
        else
          SetScrollOffset(_scrollOffsetX, _scrollOffsetY + NUM_SCROLL_PIXEL);
      return true;
    }

    public bool FocusDown()
    {
      if (!MoveFocus1(MoveFocusDirection.Down))
        // We couldn't move the focus - fallback: move physical scrolling offset
        if (IsViewPortAtBottom)
          return false;
        else
          SetScrollOffset(_scrollOffsetX, _scrollOffsetY - NUM_SCROLL_PIXEL);
      return true;
    }

    public bool FocusLeft()
    {
      if (!MoveFocus1(MoveFocusDirection.Left))
        // We couldn't move the focus - fallback: move physical scrolling offset
        if (IsViewPortAtLeft)
          return false;
        else
          SetScrollOffset(_scrollOffsetX + NUM_SCROLL_PIXEL, _scrollOffsetY);
      return true;
    }

    public bool FocusRight()
    {
      if (!MoveFocus1(MoveFocusDirection.Right))
        // We couldn't move the focus - fallback: move physical scrolling offset
        if (IsViewPortAtRight)
          return false;
        else
          SetScrollOffset(_scrollOffsetX - NUM_SCROLL_PIXEL, _scrollOffsetY);
      return true;
    }

    public bool FocusPageUp()
    {
      FrameworkElement currentElement = GetFocusedElementOrChild();
      // Try to find first element which extends our range
      while (currentElement != null &&
          (currentElement.ActualPosition.Y >= ActualPosition.Y))
        currentElement = PredictFocus(currentElement.ActualBounds, MoveFocusDirection.Up);
      if (currentElement != null)
        return currentElement.TrySetFocus(true);
      // No element to focus - fallback: move physical scrolling offset
      if (IsViewPortAtTop)
        return false;
      SetScrollOffset(_scrollOffsetX, _scrollOffsetY + (float) ActualHeight);
      return true;
    }

    public bool FocusPageDown()
    {
      FrameworkElement currentElement = GetFocusedElementOrChild();
      // Try to find first element which extends our range
      while (currentElement != null &&
          (currentElement.ActualPosition.Y + currentElement.ActualHeight <= ActualPosition.Y + ActualHeight))
        currentElement = PredictFocus(currentElement.ActualBounds, MoveFocusDirection.Down);
      if (currentElement != null)
        return currentElement.TrySetFocus(true);
      // No element to focus - fallback: move physical scrolling offset
      if (IsViewPortAtBottom)
        return false;
      SetScrollOffset(_scrollOffsetX, _scrollOffsetY - (float) ActualHeight);
      return true;
    }

    public bool FocusPageLeft()
    {
      FrameworkElement currentElement = GetFocusedElementOrChild();
      // Try to find first element which extends our range
      while (currentElement != null &&
          (currentElement.ActualPosition.X >= ActualPosition.X))
        currentElement = PredictFocus(currentElement.ActualBounds, MoveFocusDirection.Left);
      if (currentElement != null)
        return currentElement.TrySetFocus(true);
      // No element to focus - fallback: move physical scrolling offset
      if (IsViewPortAtTop)
        return false;
      SetScrollOffset(_scrollOffsetX + (float) ActualWidth, _scrollOffsetY);
      return true;
    }

    public bool FocusPageRight()
    {
      FrameworkElement currentElement = GetFocusedElementOrChild();
      // Try to find first element which extends our range
      while (currentElement != null &&
          (currentElement.ActualPosition.X + currentElement.ActualWidth <= ActualPosition.X + ActualWidth))
        currentElement = PredictFocus(currentElement.ActualBounds, MoveFocusDirection.Right);
      if (currentElement != null)
        return currentElement.TrySetFocus(true);
      // No element to focus - fallback: move physical scrolling offset
      if (IsViewPortAtRight)
        return false;
      SetScrollOffset(_scrollOffsetX - (float) ActualWidth, _scrollOffsetY);
      return true;
    }

    public bool FocusHome()
    {
      SetScrollOffset(0, 0);
      return true;
    }

    public bool FocusEnd()
    {
      if (TemplateControl == null)
        return false;
      SetScrollOffset(-(float) TemplateControl.ActualWidth, -(float) TemplateControl.ActualHeight);
      return true;
    }

    public bool ScrollDown(int numLines)
    {
      if (IsViewPortAtBottom)
        return false;
      SetScrollOffset(_scrollOffsetX, _scrollOffsetY - numLines * NUM_SCROLL_PIXEL);
      return true;
    }

    public bool ScrollUp(int numLines)
    {
      if (IsViewPortAtTop)
        return false;
      SetScrollOffset(_scrollOffsetX, _scrollOffsetY + numLines * NUM_SCROLL_PIXEL);
      return true;
    }

    #endregion

    #region IScrollInfo implementation

    public event ScrolledDlgt Scrolled;

    public bool CanScroll
    {
      get { return _canScroll; }
      set { _canScroll = value; }
    }

    public float TotalWidth
    {
      get { return TemplateControl == null ? 0 : (float) TemplateControl.ActualWidth; }
    }

    public float TotalHeight
    {
      get { return TemplateControl == null ? 0 : (float) TemplateControl.ActualHeight; }
    }

    public float ViewPortWidth
    {
      get { return (float) ActualWidth; }
    }

    public float ViewPortStartX
    {
      get { return -_actualScrollOffsetX; }
    }

    public float ViewPortHeight
    {
      get { return (float) ActualHeight; }
    }

    public float ViewPortStartY
    {
      get { return -_actualScrollOffsetY; }
    }

    public bool IsViewPortAtTop
    {
      get { return TemplateControl == null || _actualScrollOffsetY == 0; }
    }

    public bool IsViewPortAtBottom
    {
      get { return TemplateControl == null || -_actualScrollOffsetY + ActualHeight >= TotalHeight; }
    }

    public bool IsViewPortAtLeft
    {
      get { return TemplateControl == null || _actualScrollOffsetX == 0; }
    }

    public bool IsViewPortAtRight
    {
      get { return TemplateControl == null || -_actualScrollOffsetX + ActualWidth >= TotalWidth; }
    }

    #endregion
  }
}
