
using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using Thought.vCards;

namespace Tests.Samples
{

    /* ===================================================================
     * XPalmWithPhoto
     * -------------------------------------------------------------------
     * This is a unit test of a public vCard download from the Gerd
     * Leonard blog at http://www.gerdleonhard.net/.  The owner of the
     * vCard (and the web site) has no affiliation with this library.
     * 
     * This card is used as a sample because it is written in the 2.1
     * vCard format and has an embedded photo.  There are also some
     * extended properties from a palm device.
     * =================================================================== */

    [TestFixture]
    public class XPalmWithPhotoTests
    {

        [Test]
        public void CycleXPlanWithPhoto()
        {

            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.XPalmWithPhoto)));

            Helper.CycleStandard(card);

        }


        [Test]
        public void ParseXPalmWithPhoto()
        {

            // 01 BEGIN:VCARD
            // 02 VERSION:2.1
            // 03 N:Leonhard;Gerd
            // 04 FN:Gerd Leonhard
            // 05 TITLE:CEO
            // 06 ORG:ThinkAndLink, MusicFuturist, Sonific
            // 07 ADR;WORK:;;Terrassenstrasse 26;Arlesheim;;4144;Switzerland
            // 08 URL;WORK:www.gerdleonhard.com
            // 09 TEL;WORK:+41617018882
            // 10 EMAIL:gleonhard@gmail.com
            // 11 EMAIL:gerd@gerdleonhard.com
            // 12 TEL;CELL:+41797935384
            // 13 TEL;PAGER:US:14158467093
            // 14 TEL;FAX:+4112742899
            // 15 TEL:SkypeL gleonhard
            // 16 X-Palm-Custom1:www.thinkandlink.biz
            // 17 X-Palm-Custom2:www.musicfuturist.com
            // 18 X-Palm-Custom3:Skype: gleonhard
            // 19 X-Palm-Custom4:www.sonific.com
            // 20 X-PALM-IM;Yahoo:gerdleo@hotmail.com
            // 21 PHOTO;JPEG;BASE64:
            // 22    (mime data)

            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.XPalmWithPhoto)));

            // 03 N:Leonhard;Gerd

            Assert.AreEqual(
                "Leonhard",
                card.FamilyName,
                "N (family name) on line 3 failed.");

            Assert.AreEqual(
                "Gerd",
                card.GivenName,
                "N (given name) on line 3 failed.");

            // 04 FN:Gerd Leonhard

            Assert.AreEqual(
                "Gerd Leonhard",
                card.FormattedName,
                "FN on line 4 failed.");

            // 05 TITLE:CEO

            Assert.AreEqual(
                "CEO",
                card.Title,
                "TITLE on line 5 failed.");

            // 06 ORG:ThinkAndLink, MusicFuturist, Sonific

            Assert.AreEqual(
                "ThinkAndLink, MusicFuturist, Sonific",
                card.Organization,
                "ORG on line 6 failed.");

            // 07 ADR;WORK:;;Terrassenstrasse 26;Arlesheim;;4144;Switzerland

            Assert.AreEqual(
                1,
                card.DeliveryAddresses.Count,
                "One delivery address expected on line 7.");

            Assert.IsTrue(
                card.DeliveryAddresses[0].IsWork,
                "ADR on line 7 is a work address.");

            Assert.AreEqual(
                "Terrassenstrasse 26",
                card.DeliveryAddresses[0].Street,
                "ADR on line 7 has a different street address.");

            Assert.AreEqual(
                "Arlesheim",
                card.DeliveryAddresses[0].City,
                "ADR on line 7 has a different city.");

            Assert.AreEqual(
                "4144",
                card.DeliveryAddresses[0].PostalCode,
                "ADR on line 7 has a different postal code.");

            Assert.AreEqual(
                "Switzerland",
                card.DeliveryAddresses[0].Country,
                "ADR on line 7 has a different country.");

            // 08 URL;WORK:www.gerdleonhard.com

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "Only one URL is located in the file (line 8).");

            Assert.IsTrue(
                card.Websites[0].IsWorkSite,
                "URL on line 8 is a work-related web site.");

            Assert.AreEqual(
                "www.gerdleonhard.com",
                card.Websites[0].Url,
                "URL on line 8 has a different value.");

            // 09 TEL;WORK:+41617018882

            Assert.AreEqual(
                5,
                card.Phones.Count,
                "The vCard has four numbers at lines 4, 12, 13, 14 and 15.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL on line 9 is a work-related number.");

            Assert.AreEqual(
                "+41617018882",
                card.Phones[0].FullNumber,
                "TEL on line 9 has a different wrong phone number.");

            // 10 EMAIL:gleonhard@gmail.com
            // 11 EMAIL:gerd@gerdleonhard.com

            Assert.AreEqual(
                2,
                card.EmailAddresses.Count,
                "There are two email addresses beginning at line 10.");

            Assert.AreEqual(
                "gleonhard@gmail.com",
                card.EmailAddresses[0].Address,
                "EMAIL at line 10 has a different value.");

            Assert.AreEqual(
                "gerd@gerdleonhard.com",
                card.EmailAddresses[1].Address,
                "EMAIL at line 11 has a different value.");

            // 12 TEL;CELL:+41797935384

            Assert.IsTrue(
                card.Phones[1].IsCellular,
                "TEL at line 12 is a cellular phone number.");

            Assert.AreEqual(
                "+41797935384",
                card.Phones[1].FullNumber,
                "TEL at line 12 has a different value.");

            // 13 TEL;PAGER:US:14158467093

            Assert.IsTrue(
                card.Phones[2].IsPager,
                "TEL at line 13 is a pager number.");

            Assert.AreEqual(
                "US:14158467093",
                card.Phones[2].FullNumber,
                "TEL at line 13 has a different value.");

            // 14 TEL;FAX:+4112742899

            Assert.IsTrue(
                card.Phones[3].IsFax,
                "TEL at line 14 is a fax number.");

            Assert.AreEqual(
                "+4112742899",
                card.Phones[3].FullNumber,
                "TEL at line 14 has a different value.");

            // 15 TEL:SkypeL gleonhard

            Assert.AreEqual(
                "SkypeL gleonhard",
                card.Phones[4].FullNumber,
                "TEL at line 15 has a different value.");

            // Note: The X-PALM custom properties are skipped.

            // 21 PHOTO;JPEG;BASE64:

            Assert.AreEqual(
                1,
                card.Photos.Count,
                "There is a single photo starting at line 21.");

            Assert.IsTrue(
                card.Photos[0].IsLoaded,
                "The photo is embedded and therefore should have been loaded.");

            // The bitmap is 82x96.
            // The important thing is demonstrating the ability
            // to create a bitmap object from data.

            using (Bitmap bitmap = card.Photos[0].GetBitmap())
            {
                Assert.AreEqual(
                    82,
                    bitmap.Size.Width,
                    "The width is incorrect.");

                Assert.AreEqual(
                    96,
                    bitmap.Size.Height,
                    "The height is incorrect.");

            }

        }

    }
}
