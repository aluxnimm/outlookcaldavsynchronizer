using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CalDavSynchronizer.Conversions
{
  internal class RtfXamlConverter
  {

    public static string ConvertRtfToXaml (string rtfText)
    {
      var flowDocument = new FlowDocument ();
      if (string.IsNullOrEmpty (rtfText))
        return "";
      var textRange = new TextRange (flowDocument.ContentStart, flowDocument.ContentEnd);
      using (var rtfMemoryStream = new MemoryStream ())
      {
        using (var rtfStreamWriter = new StreamWriter (rtfMemoryStream))
        {
          rtfStreamWriter.Write (rtfText);
          rtfStreamWriter.Flush ();
          rtfMemoryStream.Seek (0, SeekOrigin.Begin);
          textRange.Load (rtfMemoryStream, DataFormats.Rtf);
        }
      }
      using (var rtfMemoryStream = new MemoryStream ())
      {
        textRange = new TextRange (flowDocument.ContentStart, flowDocument.ContentEnd);
        textRange.Save (rtfMemoryStream, DataFormats.Xaml);
        rtfMemoryStream.Seek (0, SeekOrigin.Begin);
        using (var rtfStreamReader = new StreamReader (rtfMemoryStream))
        {
          return rtfStreamReader.ReadToEnd ();
        }
      }
    }

    public static string ConvertXamlToRtf (string xamlText)
    {
      var flowDocument = new FlowDocument ();
      if (string.IsNullOrEmpty (xamlText))
        return "";
      var textRange = new TextRange (flowDocument.ContentStart, flowDocument.ContentEnd);
      using (var xamlMemoryStream = new MemoryStream ())
      {
        using (var xamlStreamWriter = new StreamWriter (xamlMemoryStream))
        {
          xamlStreamWriter.Write (xamlText);
          xamlStreamWriter.Flush ();
          xamlMemoryStream.Seek (0, SeekOrigin.Begin);
          textRange.Load (xamlMemoryStream, DataFormats.Xaml);
        }
      }
      using (var rtfMemoryStream = new MemoryStream ())
      {
        textRange = new TextRange (flowDocument.ContentStart, flowDocument.ContentEnd);
        textRange.Save (rtfMemoryStream, DataFormats.Rtf);
        rtfMemoryStream.Seek (0, SeekOrigin.Begin);
        using (var rtfStreamReader = new StreamReader (rtfMemoryStream))
        {
          return rtfStreamReader.ReadToEnd ();
        }
      }
    }
  }
}