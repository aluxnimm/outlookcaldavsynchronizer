using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;
using System.Linq;

namespace CalDavSynchronizer.Ui.Options.Views
{
  public class OlItemTypeToImageConverter : IValueConverter
  {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch ((OlItemType?) value)
      {
        case OlItemType.olAppointmentItem:
          return BitmapFrame.Create (new Uri ("pack://application:,,,/CalDavSynchronizer;component/Resources/folder-calendar.png"));
        case OlItemType.olTaskItem:
          return BitmapFrame.Create (new Uri ("pack://application:,,,/CalDavSynchronizer;component/Resources/folder-tasks.png"));
        case OlItemType.olContactItem:
          return BitmapFrame.Create (new Uri ("pack://application:,,,/CalDavSynchronizer;component/Resources/folder-contacts.png"));
        default:
          return Binding.DoNothing;
      }
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Binding.DoNothing;
    }
  }
}