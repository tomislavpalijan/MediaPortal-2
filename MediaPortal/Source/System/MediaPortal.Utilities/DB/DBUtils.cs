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
using System.Data;

namespace MediaPortal.Utilities.DB
{
  public class DBUtils
  {
    public static object ReadDBObject(IDataReader reader, int colIndex)
    {
      if (reader.IsDBNull(colIndex))
        return null;
      return reader.GetValue(colIndex);
    }

    public static T ReadDBValue<T>(IDataReader reader, int colIndex)
    {
      return (T) ReadDBValue(typeof(T), reader, colIndex);
    }

    public static object ReadDBValue(Type type, IDataReader reader, int colIndex)
    {
      if (reader.IsDBNull(colIndex))
        return null;
      if (type == typeof(string))
        return reader.GetString(colIndex);
      if (type == typeof(DateTime))
        return reader.GetDateTime(colIndex);
      if (type == typeof(Char))
        return reader.GetChar(colIndex);
      if (type == typeof(Boolean))
        return reader.GetBoolean(colIndex);
      if (type == typeof(Single))
        return reader.GetFloat(colIndex);
      if (type == typeof(Double))
        return reader.GetDouble(colIndex);
      if (type == typeof(Int32))
        return reader.GetInt32(colIndex);
      if (type ==typeof(Int64))
        return reader.GetInt64(colIndex);
      throw new ArgumentException(string.Format(
          "The datatype '{0}' is not supported as a DB datatype", type.Name));
    }

    public static IDbDataParameter AddParameter(IDbCommand command, object value)
    {
      IDbDataParameter result = command.CreateParameter();
      result.Value = value ?? DBNull.Value;
      command.Parameters.Add(result);
      return result;
    }
  }
}