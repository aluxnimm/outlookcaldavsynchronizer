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
using CalDavSynchronizer.DataAccess;
using log4net;
using ColorMine;
using ColorMine.ColorSpaces;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Utilities
{
  internal static class ColorHelper
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public static readonly Dictionary<OlCategoryColor, ArgbColor> ArgbColorByCategoryColor = new Dictionary<OlCategoryColor, ArgbColor>
    {
      {OlCategoryColor.olCategoryColorNone, ArgbColor.FromRgb(0xFFFFFF)},
      {OlCategoryColor.olCategoryColorRed, ArgbColor.FromRgb(0xE7A1A2)},
      {OlCategoryColor.olCategoryColorOrange, ArgbColor.FromRgb(0xF9BA89)},
      {OlCategoryColor.olCategoryColorPeach, ArgbColor.FromRgb(0xF7DD8F)},
      {OlCategoryColor.olCategoryColorYellow, ArgbColor.FromRgb(0xFCFA90)},
      {OlCategoryColor.olCategoryColorGreen, ArgbColor.FromRgb(0x78D168)},
      {OlCategoryColor.olCategoryColorTeal, ArgbColor.FromRgb(0x9FDCC9)},
      {OlCategoryColor.olCategoryColorOlive, ArgbColor.FromRgb(0xC6D2B0)},
      {OlCategoryColor.olCategoryColorBlue, ArgbColor.FromRgb(0x9DB7E8)},
      {OlCategoryColor.olCategoryColorPurple, ArgbColor.FromRgb(0xB5A1E2)},
      {OlCategoryColor.olCategoryColorMaroon, ArgbColor.FromRgb(0xdaaec2)},
      {OlCategoryColor.olCategoryColorSteel, ArgbColor.FromRgb(0xdad9dc)},
      {OlCategoryColor.olCategoryColorDarkSteel, ArgbColor.FromRgb(0x6b7994)},
      {OlCategoryColor.olCategoryColorGray, ArgbColor.FromRgb(0xbfbfbf)},
      {OlCategoryColor.olCategoryColorDarkGray, ArgbColor.FromRgb(0x6f6f6f)},
      {OlCategoryColor.olCategoryColorBlack, ArgbColor.FromRgb(0x4f4f4f)},
      {OlCategoryColor.olCategoryColorDarkRed, ArgbColor.FromRgb(0xc11a25)},
      {OlCategoryColor.olCategoryColorDarkOrange, ArgbColor.FromRgb(0xe2620d)},
      {OlCategoryColor.olCategoryColorDarkPeach, ArgbColor.FromRgb(0xc79930)},
      {OlCategoryColor.olCategoryColorDarkYellow, ArgbColor.FromRgb(0xb9b300)},
      {OlCategoryColor.olCategoryColorDarkGreen, ArgbColor.FromRgb(0x368f2b)},
      {OlCategoryColor.olCategoryColorDarkTeal, ArgbColor.FromRgb(0x329b7a)},
      {OlCategoryColor.olCategoryColorDarkOlive, ArgbColor.FromRgb(0x778b45)},
      {OlCategoryColor.olCategoryColorDarkBlue, ArgbColor.FromRgb(0x2858a5)},
      {OlCategoryColor.olCategoryColorDarkPurple, ArgbColor.FromRgb(0x5c3fa3)},
      {OlCategoryColor.olCategoryColorDarkMaroon, ArgbColor.FromRgb(0x93446b)}
    };
    
    public static OlCategoryColor FindMatchingCategoryColor(ArgbColor argbColor)
    {
      var color = Color.FromArgb (argbColor.ArgbValue);

      double minDistance = double.MaxValue;
      OlCategoryColor matchingCategoryColor = OlCategoryColor.olCategoryColorNone;

      foreach (var cat in ArgbColorByCategoryColor)
      {
        Color catColor = Color.FromArgb(cat.Value.ArgbValue);

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
