// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Globalization;
using System.Reflection;
using log4net;

namespace CalDavSynchronizer.Utilities
{
  public struct ArgbColor
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly int _argbValue;

    public ArgbColor (int argbValue)
    {
      _argbValue = argbValue;
    }

    public int ArgbValue => _argbValue;

    public static ArgbColor FromRgb (int rgb)
    {
      return new ArgbColor (0xff << 24 | rgb);
    }

    public static ArgbColor FromRgbaHexStringWithOptionalANoThrow (string rgba)
    {
      return new ArgbColor (HexToArgbColor (rgba));
    }

    public string ToRgbaHexString ()
    {
      var rbga = _argbValue << 8 | 0x000000ff & _argbValue >> 24;
      return $"#{Convert.ToString (rbga, 16)}";
    }

    private static int HexToArgbColor (string rgba)
    {
      try
      {
        string color = rgba.Replace ("#", "");
        byte r = Byte.Parse (color.Substring (0, 2), NumberStyles.HexNumber);
        byte g = Byte.Parse (color.Substring (2, 2), NumberStyles.HexNumber);
        byte b = Byte.Parse (color.Substring (4, 2), NumberStyles.HexNumber);
        byte a = (color.Length == 8) ? Byte.Parse (color.Substring (6, 2), NumberStyles.HexNumber) : Convert.ToByte (255);

        return a << 24 | r << 16 | g << 8 | b;
      }
      catch (Exception x)
      {
        s_logger.WarnFormat ("Could not parse calendar color '{0}'. Using gray", rgba);
        s_logger.Debug (x);
        // Return gray if caldav color is invalid
        return 0xff << 24 | 0x808080;
      }
    }
  }
}