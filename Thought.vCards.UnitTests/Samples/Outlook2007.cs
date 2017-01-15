
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using Thought.vCards;

namespace Tests.Samples
{
    [TestFixture]
    public class Outlook2007
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

        [Test]
        public void ParseOutlook2007()
        {

            // 01: BEGIN:VCARD
            // 02: VERSION:2.1
            // 03: N;LANGUAGE=en-us:Pinch;David;John;Mr.
            // 04: FN:Mr. David John Pinch
            // 05: NICKNAME:Dave
            // 06: ORG:Thought Project
            // 07: TITLE:Dictator
            // 08: NOTE:Generated with Outlook 2007.
            // 09: TEL;WORK;VOICE:800-929-5805
            // 10: TEL;HOME;VOICE:612-269-6017
            // 11: TEL;CELL;VOICE:612-269-6017
            // 12: ADR;WORK:;;;;;;United States of America
            // 13: ADR;HOME;PREF:;;3247 Upton Avenue N,;Minneapolis;MN;55412;United States of America
            // 14: LABEL;HOME;PREF;ENCODING=QUOTED-PRINTABLE:3247 Upton Avenue N,=0D=0A=
            // 15: Minneapolis, MN 55412
            // 16: X-MS-OL-DEFAULT-POSTAL-ADDRESS:1
            // 17: X-WAB-GENDER:2
            // 18: URL;WORK:http://www.thoughtproject.com
            // 19: ROLE:Programmer
            // 20: BDAY:20090414
            // 21: KEY;X509;ENCODING=BASE64:
            // 22:  MIICZDCCAc2gAwIBAgIQRMnJkySZCu8S15WhdyuljjANBgkqhkiG9w0BAQUFADBiMQswCQYD
            // 23:  VQQGEwJaQTElMCMGA1UEChMcVGhhd3RlIENvbnN1bHRpbmcgKFB0eSkgTHRkLjEsMCoGA1UE
            // 24:  AxMjVGhhd3RlIFBlcnNvbmFsIEZyZWVtYWlsIElzc3VpbmcgQ0EwHhcNMDcwNTExMTU0NTI2
            // 25:  WhcNMDgwNTEwMTU0NTI2WjBJMR8wHQYDVQQDExZUaGF3dGUgRnJlZW1haWwgTWVtYmVyMSYw
            // 26:  JAYJKoZIhvcNAQkBFhdkYXZlQHRob3VnaHRwcm9qZWN0LmNvbTCBnzANBgkqhkiG9w0BAQEF
            // 27:  AAOBjQAwgYkCgYEAmLhq0UDsgB8paOBzXCtv9SbwccPYJhJr6f7bK3JO1xkCbKmzoLhCZUVB
            // 28:  4zP5bWTBnQYpolA9t1Pbrd29flX90xizEljCuL2uOz4cFc+NoF7h0h5nFvnFVAmzsJmLnCop
            // 29:  vp0GD4jy3cpQhBNAoSqQbwjSuqyFtHeVMIbNlU3Y/Y8CAwEAAaM0MDIwIgYDVR0RBBswGYEX
            // 30:  ZGF2ZUB0aG91Z2h0cHJvamVjdC5jb20wDAYDVR0TAQH/BAIwADANBgkqhkiG9w0BAQUFAAOB
            // 31:  gQCuMF4mUI5NWv0DqblQH/pqJN0eCjj2j4iQhJNTHtfhrS0ETbakgldJCzg5Rv+8V2Dil7gs
            // 32:  4zMmwOuDrHVBqWDvF0/hXzMn5KKWEmzCZshVyFJ24IWkIj4t3wOMG21NdSA+zX7TEc3s7oWh
            // 33:  zi6q4lcDj3pOzUyDmaEEBYcyWLKXpA==
            // 34:
            // 35: EMAIL;PREF;INTERNET:dave@thoughtproject.com
            // 36: X-MS-IMADDRESS:davepinch@gmail.com
            // 37: PHOTO;TYPE=JPEG;ENCODING=BASE64:
            // 38:  /9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQY
            // 39:  GBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYa
            // 40:  KCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAAR
            // 41:  CAA3AEgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAA
            // 42:  AgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkK
            // 43:  FhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWG
            // 44:  h4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl
            // 45:  5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREA
            // 46:  AgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYk
            // 47:  NOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOE
            // 48:  hYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk
            // 49:  5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwC3KUf7wwaiFvuPDDHrVh0yaiMeWrarK2pw
            // 50:  wi2KLeFfvuD9KiuY4SmI1JPris3xHq0ek2jGMq1zjcseCeB3OK4+HxZdy3DPcXKwRkbVVEHX
            // 51:  8a5f3k1dLQ6Y0ktzt4oBGc7efellkYDl1UflXAf23Pf+dbC/lSR8iOTdgA9gccVys012929t
            // 52:  ctdyXCttZM96hU5vRs29nHqes3FzbqhaW6jwOD84pkSxzwiWE70PQiuJsYksIrYXcjiSWQKk
            // 53:  PmEjJOOnSu58ORMbFgfugjH1wM05R5epnKNitNbjHIorUnh5orRSdjFm6RzUNyTHDI64DAEj
            // 54:  NWShzXK/EKa4h0lI4Nw81yGYdgAa6KkeZ2FRTbsjm3uhe6m6wjzmZtssj8nnPT2r0bwl8PbX
            // 55:  WkQTJkOcMw4K1434SWeS5crkhWz9TXtWjeNL62RUsbEKYQPnKnBx159awrNqVketQgoxba1J
            // 56:  L/4GPFqUklpKGhjG4Ajlq8o8Z6i+m6rLDYpCXVArMUyQfTNfTcPxBYaJb382nSyGWQxbIsn5
            // 57:  sZ544FfKPjW6sLjxNqrQvIiPcu+NucZOcfhVRSlruYT9ptJWK/hQf2lrol1OR55kUvCB8qof
            // 58:  p3r1bw8mLGQej4/QV5n4Njt/7Zi8mV3cI2QVx2+tepaAP9Fn/wCup/kKjEJKyRzu9ncS5XBo
            // 59:  pNUYxwyMvUDiiiEbo53ubgIrN8QQefp8oVBIVG7Yeje1Le38VlA007YRRn3P0rzjVviBfS3c
            // 60:  kNrEkEe7ardWx/Ku2VCc37oUW07ot6FaLaSQ3ESnyZTnawwV7YNev6Jqun2nhq6eWKBJVjOD
            // 61:  gDJxXik+tui20wybdkCvj+Fh1qGXUDrV59ne5ZbZV+VVPDH3FctWk+bU9mnW5Vdnufwq8S6X
            // 62:  cm6tbqeNwxDqG6bhnp74r5d8R3sVzr+oz2/+rluJHQexY4r0AJpunaN9os3PmF9nQjBxya8+
            // 63:  uNEfzA0dxEQ3IzxWtKHIr9DKtUU5PubnwzzLr7Z52wk/yFev6EmLSf8A66n+Qryz4c2Mmn6p
            // 64:  cTXDJtaMKCpz1I/wr1bQmBs5COhkJrLEatHFOVmylrp227+/FFQeIXOFUHqaK0pR905Wzh9Z
            // 65:  1P8AtdWmZiVUYxyB9MVw2ptJDqLM2MqR09+9FFe7XSVNWN6as7HU+CZI7u9u7C5BZJcSLnse
            // 66:  9dLF8OWursHS7sQSMeFcZX/61FFeRXk4zdjqi/dOWuGkhlnspHDCCZ4zjoSDg4/nV/wJNC+t
            // 67:  xW91Ek0UoZGV1yMjoR+VFFdc4r2TXkYSep2euafZ6ftlsrdY95+YLxnFa+iOY9MUHqSTRRXl
            // 68:  PWKMZtsyNal3Tge1FFFdtKK5UZM//9k=
            // 69:
            // 70: X-MS-OL-DESIGN;CHARSET=utf-8:<card xmlns="http://schemas.microsoft.com/office/outlook/12/electronicbusinesscards" ver="1.0" layout="left" bgcolor="ffffff"><img xmlns="" align="tleft" area="32" use="photo"/><fld xmlns="" prop="name" align="left" dir="ltr" style="b" color="000000" size="10"/><fld xmlns="" prop="org" align="left" dir="ltr" color="000000" size="8"/><fld xmlns="" prop="title" align="left" dir="ltr" color="000000" size="8"/><fld xmlns="" prop="telwork" align="left" dir="ltr" color="000000" size="8"><label align="right" color="626262">Work</label></fld><fld xmlns="" prop="telcell" align="left" dir="ltr" color="000000" size="8"><label align="right" color="626262">Mobile</label></fld><fld xmlns="" prop="telhome" align="left" dir="ltr" color="000000" size="8"><label align="right" color="626262">Home</label></fld><fld xmlns="" prop="email" align="left" dir="ltr" color="000000" size="8"/><fld xmlns="" prop="addrhome" align="left" dir="ltr" color="000000" size="8"/><fld xmlns="" prop="webwork" align="left" dir="ltr" color="000000" size="8"/><fld xmlns="" prop="blank" size="8"/><fld xmlns="" prop="blank" size="8"/><fld xmlns="" prop="blank" size="8"/><fld xmlns="" prop="blank" size="8"/><fld xmlns="" prop="blank" size="8"/><fld xmlns="" prop="blank" size="8"/><fld xmlns="" prop="blank" size="8"/></card>
            // 71: REV:20090825T194011Z
            // 72: END:VCARD

            vCard card = new vCard(
                new StreamReader(new MemoryStream(SampleCards.Outlook2007)));

            // 03: N:Pinch;David;John

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

            // 04: FN:Mr. David John Pinch

            Assert.AreEqual(
                "Mr. David John Pinch",
                card.FormattedName,
                "FN at line 4 has a different formatted name.");

            // 05: NICKNAME:Dave

            Assert.AreEqual(
                1,
                card.Nicknames.Count,
                "Exactly one nickname is located at line 5.");

            Assert.AreEqual(
                "Dave",
                card.Nicknames[0],
                "NICKNAME at line 5 has a different value.");

            // 06: ORG:Thought Project

            Assert.AreEqual(
                "Thought Project",
                card.Organization,
                "ORG at line 6 has a different value.");

            // 07: TITLE:Dictator

            Assert.AreEqual(
                "Dictator",
                card.Title,
                "TITLE at line 7 has a different value.");

            // 08: NOTE:Generated with Outlook 2007.

            Assert.AreEqual(
                1,
                card.Notes.Count,
                "Only one note should be in the card.");

            Assert.AreEqual(
                "Generated with Outlook 2007.",
                card.Notes[0].Text,
                "NOTE at line 8 has a different value.");

            // 09: TEL;WORK;VOICE:800-929-5805

            Assert.AreEqual(
                3,
                card.Phones.Count,
                "Three phone numbers should have been loaded.");

            Assert.AreEqual(
                "800-929-5805",
                card.Phones[0].FullNumber,
                "TEL at line 9 has the wrong phone number.");

            Assert.AreEqual(
                vCardPhoneTypes.WorkVoice,
                card.Phones[0].PhoneType,
                "TEL at line 9 has the wrong phone types.");

            // 10: TEL;HOME;VOICE:612-269-6017

            Assert.AreEqual(
                "612-269-6017",
                card.Phones[1].FullNumber,
                "TEL at line 10 has the wrong phone number.");

            Assert.AreEqual(
                vCardPhoneTypes.HomeVoice,
                card.Phones[1].PhoneType,
                "TEL at line 10 has the wrong phone types.");

            // 11: TEL;CELL;VOICE:612-269-6017

            Assert.AreEqual(
                "612-269-6017",
                card.Phones[2].FullNumber,
                "TEL at line 11 has the wrong phone number.");

            Assert.AreEqual(
                vCardPhoneTypes.CellularVoice,
                card.Phones[2].PhoneType,
                "TEL at line 11 has the wrong phone types.");

            // 12: ADR;WORK:;;;;;;United States of America

            Assert.AreEqual(
                2,
                card.DeliveryAddresses.Count,
                "Two delivery addresses should be defined.");

            Assert.AreEqual(
                vCardDeliveryAddressTypes.Work,
                card.DeliveryAddresses[0].AddressType,
                "ADR on line 12 has the wrong address type.");

            Assert.AreEqual(
                string.Empty,
                card.DeliveryAddresses[0].City,
                "ADR on line 12 should have a blank city.");

            Assert.AreEqual(
                "United States of America",
                card.DeliveryAddresses[0].Country,
                "ADR on line 12 has the wrong country.");

            Assert.AreEqual(
                string.Empty,
                card.DeliveryAddresses[0].PostalCode,
                "ADR on line 12 should have a blank postal code.");

            Assert.AreEqual(
                string.Empty,
                card.DeliveryAddresses[0].Region,
                "ADR on line 12 should have a blank region.");

            Assert.AreEqual(
                string.Empty,
                card.DeliveryAddresses[0].Street,
                "ADR on line 12 should have a blank street.");

            // 13: ADR;HOME;PREF:;;3247 Upton Avenue N,;Minneapolis;MN;55412;United States of America

            Assert.AreEqual(
                "Minneapolis",
                card.DeliveryAddresses[1].City,
                "ADR on line 13 has the wrong city.");

            Assert.AreEqual(
                "United States of America",
                card.DeliveryAddresses[1].Country,
                "ADR on line 13 has the wrong country.");

            Assert.IsTrue(
                card.DeliveryAddresses[1].IsHome,
                "ADR on line 13 should be a home address.");

            Assert.AreEqual(
                "55412",
                card.DeliveryAddresses[1].PostalCode,
                "ADR on line 13 has the wrong postal code.");

            Assert.AreEqual(
                card.DeliveryAddresses[1].Region,
                "MN",
                "ADR on line 13 has the wrong region (state/province).");

            Assert.AreEqual(
                "3247 Upton Avenue N,",
                card.DeliveryAddresses[1].Street,
                "ADR on line 13 has the wrong street.");

            // 14: LABEL;HOME;PREF;ENCODING=QUOTED-PRINTABLE:3247 Upton Avenue N,=0D=0A=
            // 15: Minneapolis, MN 55412

            Assert.AreEqual(
                1,
                card.DeliveryLabels.Count,
                "LABEL on line 14-15 should be the only delivery label.");

            Assert.AreEqual(
                "3247 Upton Avenue N,\r\nMinneapolis, MN 55412",
                card.DeliveryLabels[0].Text,
                "LABEL on lines 14-15 does not match.");

            // 16: X-MS-OL-DEFAULT-POSTAL-ADDRESS:1

            // Unknown? Need to look this up.

            // 17: X-WAB-GENDER:2

            Assert.AreEqual(
                vCardGender.Male,
                card.Gender,
                "X-WAB-GENDER on line 17 should indicate the male gender.");

            // 18: URL;WORK:http://www.thoughtproject.com

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "URL on line 18 should be the only website in the card.");

            Assert.IsTrue(
                card.Websites[0].IsWorkSite,
                "URL on line 18 indicates a work site.");

            Assert.AreEqual(
                "http://www.thoughtproject.com",
                card.Websites[0].Url,
                "URL on line 18 mismatch.");

            // 19: ROLE:Programmer

            Assert.AreEqual(
                "Programmer",
                card.Role,
                "ROLE on line 19 mismatch.");

            // 20: BDAY:20090414

            Assert.IsNotNull(
                card.BirthDate,
                "BDAY on line 20 should have been loaded (property is null).");

            Assert.AreEqual(
                new DateTime(2009, 4, 14),
                card.BirthDate.Value,
                "BDAY on line 20 mismatch.");

            // 21: KEY;X509;ENCODING=BASE64:

            Assert.AreEqual(
                1,
                card.Certificates.Count,
                "KEY on line 21 should be the only certificate.");

            Assert.AreEqual(
                "X509",
                card.Certificates[0].KeyType,
                "KEY on line 21 has the wrong key type.");

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

            // 35: EMAIL;PREF;INTERNET:dave@thoughtproject.com

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "EMAIL on line 35 should be the only email address.");

            Assert.AreEqual(
                "dave@thoughtproject.com",
                card.EmailAddresses[0].Address,
                "EMAIL on line 35 has the wrong email address.");

            Assert.IsTrue(
                card.EmailAddresses[0].IsPreferred,
                "EMAIL on line 35 should be preferred.");

            Assert.AreEqual(
                vCardEmailAddressType.Internet,
                card.EmailAddresses[0].EmailType,
                "EMAIL on line 35 should be an Internet email address.");

            // 36: X-MS-IMADDRESS:davepinch@gmail.com

            // Not supported (yet)

            // 37: PHOTO;TYPE=JPEG;ENCODING=BASE64:

            Assert.AreEqual(
                1,
                card.Photos.Count,
                "PHOTO on line 37 should be the only photo.");

            // This should work without exception
            using(card.Photos[0].GetBitmap())
            {
            }


        }

    }
}
