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
using ColorMine;
using ColorMine.ColorSpaces;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Utilities
{
  internal static class ColorHelper
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public static readonly Dictionary<OlCategoryColor, string> CategoryColors = new Dictionary<OlCategoryColor, string>
    {
      {OlCategoryColor.olCategoryColorNone, "#FFFFFF"},
      {OlCategoryColor.olCategoryColorRed, "#E7A1A2"},
      {OlCategoryColor.olCategoryColorOrange, "#F9BA89"},
      {OlCategoryColor.olCategoryColorPeach, "#F7DD8F"},
      {OlCategoryColor.olCategoryColorYellow, "#FCFA90"},
      {OlCategoryColor.olCategoryColorGreen, "#78D168"},
      {OlCategoryColor.olCategoryColorTeal, "#9FDCC9"},
      {OlCategoryColor.olCategoryColorOlive, "#C6D2B0"},
      {OlCategoryColor.olCategoryColorBlue, "#9DB7E8"},
      {OlCategoryColor.olCategoryColorPurple, "#B5A1E2"},
      {OlCategoryColor.olCategoryColorMaroon, "#daaec2"},
      {OlCategoryColor.olCategoryColorSteel, "#dad9dc"},
      {OlCategoryColor.olCategoryColorDarkSteel, "#6b7994"},
      {OlCategoryColor.olCategoryColorGray, "#bfbfbf"},
      {OlCategoryColor.olCategoryColorDarkGray, "#6f6f6f"},
      {OlCategoryColor.olCategoryColorBlack, "#4f4f4f"},
      {OlCategoryColor.olCategoryColorDarkRed, "#c11a25"},
      {OlCategoryColor.olCategoryColorDarkOrange, "#e2620d"},
      {OlCategoryColor.olCategoryColorDarkPeach, "#c79930"},
      {OlCategoryColor.olCategoryColorDarkYellow, "#b9b300"},
      {OlCategoryColor.olCategoryColorDarkGreen, "#368f2b"},
      {OlCategoryColor.olCategoryColorDarkTeal, "#329b7a"},
      {OlCategoryColor.olCategoryColorDarkOlive, "#778b45"},
      {OlCategoryColor.olCategoryColorDarkBlue, "#2858a5"},
      {OlCategoryColor.olCategoryColorDarkPurple, "#5c3fa3"},
      {OlCategoryColor.olCategoryColorDarkMaroon, "#93446b"}
    };


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
      catch (System.Exception x)
      {
        s_logger.WarnFormat ("Could not parse calendar color '{0}'. Using gray", hexColor);
        s_logger.Debug (x);
        // Return gray if caldav color is invalid
        return Color.Gray;
      }
    }

    public static OlCategoryColor FindMatchingCategoryColor(Color color)
    {

      double minDistance = double.MaxValue;
      OlCategoryColor matchingCategoryColor = OlCategoryColor.olCategoryColorNone;

      foreach (var cat in CategoryColors)
      {
        Color catColor = HexToColor(cat.Value);

        var a = new Rgb { R = color.R, G = color.G, B = color.B };
        var b = new Rgb { R = catColor.R, G = catColor.G, B = catColor.B };

        double curDistance = a.Compare(b, new ColorMine.ColorSpaces.Comparisons.CieDe2000Comparison());

        if (curDistance < minDistance)
        {
          minDistance = curDistance;
          matchingCategoryColor = cat.Key;
        }
      }

      return matchingCategoryColor;
    }

  }
}
