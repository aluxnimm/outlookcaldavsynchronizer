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
        var values = BitConverter.GetBytes (ColorHelper.CategoryColors[(OlCategoryColor) value]);
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