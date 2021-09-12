using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace CalDavSynchronizer.Conversions
{
    internal class RtfXamlConverter
    {
        public static string ConvertRtfToXaml(string rtfText)
        {
            if (string.IsNullOrEmpty(rtfText))
                return "";

            var flowDocument = new FlowDocument();
            var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            using (var rtfMemoryStream = new MemoryStream())
            {
                using (var rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                {
                    rtfStreamWriter.Write(rtfText);
                    rtfStreamWriter.Flush();
                    rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                    textRange.Load(rtfMemoryStream, DataFormats.Rtf);
                }
            }

            return XamlWriter.Save(flowDocument);
        }

        public static string ConvertXamlToRtf(string xamlText)
        {
            FlowDocument flowDocument;

            using (var xamlTextReader = new StringReader(xamlText))
            using (var xamlXmlReader = new XmlTextReader(xamlTextReader))
            {
                flowDocument = (FlowDocument) XamlReader.Load(xamlXmlReader);
                flowDocument.SetValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
            }

            using (var rtfMemoryStream = new MemoryStream())
            {
                var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Save(rtfMemoryStream, DataFormats.Rtf);
                rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                using (var rtfStreamReader = new StreamReader(rtfMemoryStream))
                {
                    return rtfStreamReader.ReadToEnd();
                }
            }
        }
    }
}