using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CalDavSynchronizer.Conversions.UnitTests
{
    [TestFixture]
    public class DocumentConverterFixture
    {
        [Test]
        public void ConvertRtfToHtml()
        {
            var html = new DocumentConverter().ConvertRtfToHtml(@"{\rtf1
 Guten Tag!
 \line
 {\i Dies} ist \b{\i ein 
 \i0 formatierter \b0Text}.
 \par
 \i0 Das \b0Ende.
 }");

            var rtf = new DocumentConverter().ConvertHtmlToRtf(html);
        }


        [Test]
        public void TestHtmlLinkRoundTrip()
        {
            var html = "<A HREF=\"http://www.orf.at\">Orf</A>";
            var rtf = new DocumentConverter().ConvertHtmlToRtf(html);
            var htmlRoundTrip = new DocumentConverter().ConvertRtfToHtml(rtf);
            Assert.That(htmlRoundTrip.Contains("<A HREF"));
        }
    }
}