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
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Events
{
  public class ColorMapper
  {
    public static string MapCategoryColorToHtmlColor(OlCategoryColor color)
    {
      return HtmlColorByOutlookColor[color].Name;
    }

    public static OlCategoryColor MapHtmlColorToCategoryColor(string htmlColor)
    {
      if (!OutlookColorByHtmlColorRgb.TryGetValue(ColorHelper.GetRgbValue(htmlColor), out var categoryColor))
      {
        categoryColor = ColorHelper.FindMatchingOutlookColorByHtmlColor(htmlColor);
      }
      return categoryColor;
    }

    private static readonly Dictionary<OlCategoryColor, X11Color> HtmlColorByOutlookColor = new Dictionary<OlCategoryColor, X11Color>
    {
      {OlCategoryColor.olCategoryColorRed, X11Colors.Red},
      {OlCategoryColor.olCategoryColorOrange, X11Colors.Orange},
      {OlCategoryColor.olCategoryColorPeach, X11Colors.Peachpuff},
      {OlCategoryColor.olCategoryColorYellow, X11Colors.Yellow},
      {OlCategoryColor.olCategoryColorGreen, X11Colors.Green},
      {OlCategoryColor.olCategoryColorTeal, X11Colors.Lightseagreen},
      {OlCategoryColor.olCategoryColorOlive, X11Colors.Olive},
      {OlCategoryColor.olCategoryColorBlue, X11Colors.Blue},
      {OlCategoryColor.olCategoryColorPurple, X11Colors.Purple},
      {OlCategoryColor.olCategoryColorMaroon, X11Colors.Maroon},
      {OlCategoryColor.olCategoryColorSteel, X11Colors.Lightsteelblue},
      {OlCategoryColor.olCategoryColorDarkSteel, X11Colors.Steelblue},
      {OlCategoryColor.olCategoryColorGray, X11Colors.Gray},
      {OlCategoryColor.olCategoryColorDarkGray, X11Colors.Darkgray},
      {OlCategoryColor.olCategoryColorBlack, X11Colors.Black},
      {OlCategoryColor.olCategoryColorDarkRed, X11Colors.Darkred},
      {OlCategoryColor.olCategoryColorDarkOrange, X11Colors.Darkorange},
      {OlCategoryColor.olCategoryColorDarkPeach, X11Colors.Peru},
      {OlCategoryColor.olCategoryColorDarkYellow, X11Colors.Yellowgreen},
      {OlCategoryColor.olCategoryColorDarkGreen, X11Colors.Darkgreen},
      {OlCategoryColor.olCategoryColorDarkTeal, X11Colors.Teal},
      {OlCategoryColor.olCategoryColorDarkOlive, X11Colors.Darkolivegreen},
      {OlCategoryColor.olCategoryColorDarkBlue, X11Colors.Darkblue},
      {OlCategoryColor.olCategoryColorDarkPurple, X11Colors.Darkviolet},
      {OlCategoryColor.olCategoryColorDarkMaroon, X11Colors.Palevioletred}
    };

    private static readonly Dictionary<int, OlCategoryColor> OutlookColorByHtmlColorRgb = HtmlColorByOutlookColor.ToDictionary(e => e.Value.Rgb, e => e.Key);

    public static readonly HashSet<string> HtmlColorNames = new HashSet<string>(HtmlColorByOutlookColor.Values.Select(v => v.Name), StringComparer.InvariantCultureIgnoreCase);
  }
}