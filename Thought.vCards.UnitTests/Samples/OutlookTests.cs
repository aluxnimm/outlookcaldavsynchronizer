using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;

namespace Tests.Samples
{
    /* ===================================================================
     * OutlookTests
     * -------------------------------------------------------------------
     * Tests for Outlook-generated vCards.
     * =================================================================== */

    [TestClass]
    public sealed class OutlookTests : IDisposable
    {
        /// <summary>
        ///     The issuing organization of the certificate embedded
        ///     into the Outlook vCard.
        /// </summary>
        public const string KeyIssuer =
            "CN=Thawte Personal Freemail Issuing CA, O=Thawte Consulting (Pty) Ltd., C=ZA";


        /// <summary>
        ///     The subject of the certificate.
        /// </summary>
        public const string KeySubject =
            "E=dave@thoughtproject.com, CN=Thawte Freemail Member";


        #region [ CycleOutlookCertificate ]

        [TestMethod]
        public void CycleOutlookCertificate()
        {
            // Load the vCard with the test certificate.

            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.OutlookCertificate)));

            Helper.CycleStandard(card);
        }

        #endregion

        #region [ CycleOutlookSimple ]

        [TestMethod]
        public void CycleOutlookSimple()
        {
            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.OutlookSimple)));

            Helper.CycleStandard(card);
        }

        #endregion

        #region [ ParseOutlookCertificate ]

        [TestMethod]
        public void ParseOutlookCertificate()
        {
            // 01 BEGIN:VCARD
            // 02 VERSION:2.1
            // 03 N:Pinch;David;John
            // 04 FN:David John Pinch
            // 05 NICKNAME:Dave
            // 06 ORG:Thought Project
            // 07 TITLE:Dictator
            // 08 TEL;WORK;VOICE:800-929-5805
            // 09 TEL;HOME;VOICE:612-269-6017
            // 10 KEY;X509;ENCODING=BASE64:
            // 11    MIICZDCCAc2gAwIBAgIQRMnJkySZCu8S15WhdyuljjANBgkqhkiG9w0BAQUFADBiMQswCQYD
            // 12   VQQGEwJaQTElMCMGA1UEChMcVGhhd3RlIENvbnN1bHRpbmcgKFB0eSkgTHRkLjEsMCoGA1UE
            // 13    AxMjVGhhd3RlIFBlcnNvbmFsIEZyZWVtYWlsIElzc3VpbmcgQ0EwHhcNMDcwNTExMTU0NTI2
            // 14    WhcNMDgwNTEwMTU0NTI2WjBJMR8wHQYDVQQDExZUaGF3dGUgRnJlZW1haWwgTWVtYmVyMSYw
            // 15    JAYJKoZIhvcNAQkBFhdkYXZlQHRob3VnaHRwcm9qZWN0LmNvbTCBnzANBgkqhkiG9w0BAQEF
            // 16    AAOBjQAwgYkCgYEAmLhq0UDsgB8paOBzXCtv9SbwccPYJhJr6f7bK3JO1xkCbKmzoLhCZUVB
            // 17    4zP5bWTBnQYpolA9t1Pbrd29flX90xizEljCuL2uOz4cFc+NoF7h0h5nFvnFVAmzsJmLnCop
            // 18    vp0GD4jy3cpQhBNAoSqQbwjSuqyFtHeVMIbNlU3Y/Y8CAwEAAaM0MDIwIgYDVR0RBBswGYEX
            // 19    ZGF2ZUB0aG91Z2h0cHJvamVjdC5jb20wDAYDVR0TAQH/BAIwADANBgkqhkiG9w0BAQUFAAOB
            // 20    gQCuMF4mUI5NWv0DqblQH/pqJN0eCjj2j4iQhJNTHtfhrS0ETbakgldJCzg5Rv+8V2Dil7gs
            // 21    4zMmwOuDrHVBqWDvF0/hXzMn5KKWEmzCZshVyFJ24IWkIj4t3wOMG21NdSA+zX7TEc3s7oWh
            // 22    zi6q4lcDj3pOzUyDmaEEBYcyWLKXpA==
            // 23
            // 24
            // 25 EMAIL;PREF;INTERNET:dave@thoughtproject.com
            // 26 REV:20070511T163814Z
            // 27 END:VCARD

            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.OutlookCertificate)));

            // 03 N:Pinch;David;John

            Assert.AreEqual(
                "Pinch",
                card.FamilyName,
                "N at line 3 has a different family name.");

            Assert.AreEqual(
                "David",
                card.GivenName,
                "N at line 3 has a different given name.");

            Assert.AreEqual(
                "John",
                card.AdditionalNames,
                "N at line 3 has a different middle name.");

            Assert.AreEqual(
                1,
                card.Certificates.Count,
                "Only one certificate was expected.");

            // 04 FN:David John Pinch

            Assert.AreEqual(
                "David John Pinch",
                card.FormattedName,
                "FN at line 4 has a different formatted name.");

            // 05 NICKNAME:Dave

            Assert.AreEqual(
                1,
                card.Nicknames.Count,
                "Exactly one nickname is located at line 5.");

            Assert.AreEqual(
                "Dave",
                card.Nicknames[0],
                "NICKNAME at line 5 has a different value.");

            // 06 ORG:Thought Project

            Assert.AreEqual(
                "Thought Project",
                card.Organization,
                "ORG at line 6 has a different value.");

            // 07 TITLE:Dictator

            Assert.AreEqual(
                "Dictator",
                card.Title,
                "TITLE at line 7 has a different value.");

            // 08 TEL;WORK;VOICE:800-929-5805
            // 09 TEL;HOME;VOICE:612-269-6017

            Assert.AreEqual(
                2,
                card.Phones.Count,
                "Two telephone numbers are defined at lines 8 and 9.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL at line 8 is a work number.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL at line 8 is a voice number.");

            Assert.AreEqual(
                "800-929-5805",
                card.Phones[0].FullNumber,
                "TEL at line 8 has a different value.");

            // 09 TEL;HOME;VOICE:612-269-6017

            Assert.IsTrue(
                card.Phones[1].IsHome,
                "TEL at line 9 is a home number.");

            Assert.IsTrue(
                card.Phones[1].IsVoice,
                "TEL at line 9 is a voice number.");

            Assert.AreEqual(
                "612-269-6017",
                card.Phones[1].FullNumber,
                "TEL at line 9 has a different value.");

            // 10 KEY;X509;ENCODING=BASE64:

            Assert.AreEqual(
                1,
                card.Certificates.Count,
                "There is one certificate starting on line 10.");

            Assert.AreEqual(
                "X509",
                card.Certificates[0].KeyType,
                "KEY on line 10 has a different key type.");

            // Create an instance of the certificate.

            X509Certificate2 cert =
                new X509Certificate2(card.Certificates[0].Data);

            Assert.AreEqual(
                KeyIssuer,
                cert.Issuer,
                "The key issuer has a different value.");

            Assert.AreEqual(
                KeySubject,
                cert.Subject,
                "The key subject has a different value.");
        }

        #endregion

        #region [ ParseOutlookSimple ]

        [TestMethod]
        public void ParseOutlookSimple()
        {
            // 01 BEGIN:VCARD
            // 02 VERSION:2.1
            // 03 N:Pinch;David;John
            // 04 FN:David John Pinch
            // 05 NICKNAME:Dave
            // 06 ORG:Thought Project
            // 07 TITLE:Dictator
            // 08 TEL;WORK;VOICE:800-929-5805
            // 09 TEL;HOME;VOICE:612-269-6017
            // 10 ADR;HOME:;;129 15th Street #3;Minneapolis;MN;55403;United States of America
            // 11 LABEL;HOME;ENCODING=QUOTED-PRINTABLE:129 15th Street #3=0D=0AMinneapolis, MN 55403=0D=0AUnited States of America
            // 12 URL;WORK:http://www.thoughtproject.com
            // 13 EMAIL;PREF;INTERNET:dave@thoughtproject.com
            // 14 REV:20061130T234000Z
            // 15 END:VCARD

            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.OutlookSimple)));

            // 03 N:Pinch;David;John

            Assert.AreEqual(
                "Pinch",
                card.FamilyName,
                "N at line 3 has a different family name.");

            Assert.AreEqual(
                "David",
                card.GivenName,
                "N at line 3 has a different given name.");

            Assert.AreEqual(
                "John",
                card.AdditionalNames,
                "N at line 3 has a different middle name.");

            // 04 FN:David John Pinch

            Assert.AreEqual(
                "David John Pinch",
                card.FormattedName,
                "FN at line 4 has a different formatted name.");

            // 05 NICKNAME:Dave

            Assert.AreEqual(
                1,
                card.Nicknames.Count,
                "Exactly one nickname is located at line 5.");

            Assert.AreEqual(
                "Dave",
                card.Nicknames[0],
                "NICKNAME at line 5 has a different value.");

            // 06 ORG:Thought Project

            Assert.AreEqual(
                "Thought Project",
                card.Organization,
                "ORG at line 6 has a different value.");

            // 07 TITLE:Dictator

            Assert.AreEqual(
                "Dictator",
                card.Title,
                "TITLE at line 7 has a different value.");

            // 08 TEL;WORK;VOICE:800-929-5805
            // 09 TEL;HOME;VOICE:612-269-6017

            Assert.AreEqual(
                2,
                card.Phones.Count,
                "Two telephone numbers are defined at lines 8 and 9.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL at line 8 is a work number.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL at line 8 is a voice number.");

            Assert.AreEqual(
                "800-929-5805",
                card.Phones[0].FullNumber,
                "TEL at line 8 has a different value.");

            // 09 TEL;HOME;VOICE:612-269-6017

            Assert.IsTrue(
                card.Phones[1].IsHome,
                "TEL at line 9 is a home number.");

            Assert.IsTrue(
                card.Phones[1].IsVoice,
                "TEL at line 9 is a voice number.");

            Assert.AreEqual(
                "612-269-6017",
                card.Phones[1].FullNumber,
                "TEL at line 9 has a different value.");

            // 10 ADR;HOME:;;129 15th Street #3;Minneapolis;MN;55403;United States of America

            Assert.AreEqual(
                1,
                card.DeliveryAddresses.Count,
                "There is one delivery address at line 10.");

            Assert.AreEqual(
                "129 15th Street #3",
                card.DeliveryAddresses[0].Street,
                "ADR at line 10 has a different street.");

            Assert.AreEqual(
                "Minneapolis",
                card.DeliveryAddresses[0].City,
                "ADR at line 10 has a different city.");

            Assert.AreEqual(
                "MN",
                card.DeliveryAddresses[0].Region,
                "ADR at line 10 has a different region/state.");

            Assert.AreEqual(
                "55403",
                card.DeliveryAddresses[0].PostalCode,
                "ADR at line 10 has a different postal code.");

            Assert.AreEqual(
                "United States of America",
                card.DeliveryAddresses[0].Country,
                "ADR at line 10 has a different country.");

            // 11 LABEL;HOME;ENCODING=QUOTED-PRINTABLE:129 15th Street #3=0D=0AMinneapolis, MN 55403=0D=0AUnited States of America

            Assert.AreEqual(
                1,
                card.DeliveryLabels.Count,
                "There is one delivery label at line 11.");

            Assert.IsTrue(
                card.DeliveryLabels[0].IsHome,
                "LABEL at line 11 is a home delivery address.");

            Assert.AreEqual(
                "129 15th Street #3\r\nMinneapolis, MN 55403\r\nUnited States of America",
                card.DeliveryLabels[0].Text,
                "LABEL at line 11 has a different decoded value.");

            // 12 URL;WORK:http://www.thoughtproject.com

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "There is one web site (URL) at line 12.");

            Assert.IsTrue(
                card.Websites[0].IsWorkSite,
                "The web site at line 12 is a work-related web site.");

            Assert.AreEqual(
                "http://www.thoughtproject.com",
                card.Websites[0].Url,
                "URL at line 12 has a different value.");

            // 13 EMAIL;PREF;INTERNET:dave@thoughtproject.com

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "There is one email address at line 13.");

            Assert.IsTrue(
                card.EmailAddresses[0].IsPreferred,
                "The email address at line 13 is the preferred email address.");

            Assert.AreEqual(
                "dave@thoughtproject.com",
                card.EmailAddresses[0].Address,
                "EMAIL at line 13 has a different value.");

            // 14 REV:20061130T234000Z

            Assert.AreEqual(
                vCardStandardReader.ParseDate("20061130T234000Z").Value,
                card.RevisionDate.Value,
                "REV at line 14 has a different value.");
        }

        #endregion

        #region [ ParseOutlookSimple ]

        [TestMethod]
        public void ParseUnicodeSimple()
        {
            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.UnicodeNameSample)));

            Assert.IsNotNull(card);
            //Assert.AreEqual("³ÂÀö¾ý", card.GivenName);
        }

        #endregion

        public void Dispose()
        {
            //driver.Dispose(); 
        }
    }
}