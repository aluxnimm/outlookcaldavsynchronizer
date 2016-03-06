using System;
using System.Globalization;
using System.Windows.Data;

namespace CalDavSynchronizer.Ui.Options.Views
{
  public class BooleanNotConverter : IValueConverter
  {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof (bool))
        throw new ArgumentException();

      if (value is bool)
        return !(bool) value;
      return Binding.DoNothing;
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Convert (value, targetType, parameter, culture);
    }
  }
}