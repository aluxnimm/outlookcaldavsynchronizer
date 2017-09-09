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
using System.Windows.Data;
using System.Windows.Media;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.Views
{
  public class OlCategoryColorConverter : IValueConverter
  {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof (Brush))
        throw new ArgumentException ();

      if (value is OlCategoryColor)
      {
        var values = BitConverter.GetBytes (ColorHelper.ArgbColorByCategoryColor[(OlCategoryColor) value].ArgbValue);
        var color = Color.FromArgb (values[3], values[2], values[1], values[0]);
        var brush = new SolidColorBrush (color);
        brush.Freeze();
        return brush;
      }
      else
      {
        return Binding.DoNothing;
      }
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Convert (value, targetType, parameter, culture);
    }
  }
}