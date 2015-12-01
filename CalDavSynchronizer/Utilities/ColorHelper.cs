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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Data;
using System.Reflection;
using log4net;

namespace CalDavSynchronizer.Utilities
{
  internal static class ColorHelper
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public static Color HexToColor (string hexColor)
    {
      try
      {
        string color = hexColor.Replace ("#", "");
        byte r = Byte.Parse (color.Substring (0, 2), NumberStyles.HexNumber);
        byte g = Byte.Parse (color.Substring (2, 2), NumberStyles.HexNumber);
        byte b = Byte.Parse (color.Substring (4, 2), NumberStyles.HexNumber);
        byte a = (color.Length == 8) ? Byte.Parse (color.Substring (6, 2), NumberStyles.HexNumber) : Convert.ToByte (255);

        return Color.FromArgb (a, r, g, b);
      }
      catch (Exception x)
      {
        s_logger.WarnFormat ("Could not parse calendar color '{0}'. Using gray", hexColor);
        s_logger.Debug (x);
        // Return gray if caldav color is invalid
        return Color.Gray;
      }
    }
  }
}
