
using System;
using System.IO;
using NUnit.Framework;
using Thought.vCards;

namespace Tests
{
    [TestFixture]
    public class RfcAuthorTests
    {

        [Test]
        public void CycleRfcAuthors()
        {

            using (StreamReader reader = new StreamReader(
                new MemoryStream(SampleCards.RfcAuthors)))
            {

                vCard card1 = new vCard(reader);
                vCard card2 = new vCard(reader);

                Helper.CycleStandard(card1);
                Helper.CycleStandard(card2);

            }

        }


        // The RfcAuthors.txt file contains two vCard files
        // from RFC 2426 (the authors of the vCard 3.0 specification).
        // The vCards are in 3.0 format.

        [Test]
        public void ParseRfcAuthors()
        {

            using (StreamReader reader = new StreamReader(
                new MemoryStream(SampleCards.RfcAuthors)))
            {

                vCard card1 = new vCard(reader);
                vCard card2 = new vCard(reader);

                _parseCard1(card1);
                _parseCard2(card2);

            }


        }

        #region [ _parseCard1 ]

        private void _parseCard1(vCard card)
        {

            // 01 BEGIN:vCard
            // 02 VERSION:3.0
            // 03 FN:Frank Dawson
            // 04 ORG:Lotus Development Corporation
            // 05 ADR;TYPE=WORK,POSTAL,PARCEL:;;6544 Battleford Drive
            // 06  ;Raleigh;NC;27613-3502;U.S.A.
            // 07 TEL;TYPE=VOICE,MSG,WORK:+1-919-676-9515
            // 08 TEL;TYPE=FAX,WORK:+1-919-676-9564
            // 09 EMAIL;TYPE=INTERNET,PREF:Frank_Dawson@Lotus.com
            // 10 EMAIL;TYPE=INTERNET:fdawson@earthlink.net
            // 11 URL:http://home.earthlink.net/~fdawson
            // 12 END:vCard

            Assert.AreEqual(
                "Frank Dawson",
                card.FormattedName,
                "FN on line 3 is different.");

            Assert.AreEqual(
                "Lotus Development Corporation",
                card.Organization,
                "ORG on line 4 is different.");

            Assert.AreEqual(
                1,
                card.DeliveryAddresses.Count,
                "One address expected in card 1 at line 5.");

            // 05 ADR;TYPE=WORK,POSTAL,PARCEL:;;6544 Battleford Drive
            // 06  ;Raleigh;NC;27613-3502;U.S.A.

            Assert.IsTrue(
                card.DeliveryAddresses[0].IsWork,
                "ADR on lines 5-6 is a work address.");

            Assert.IsTrue(
                card.DeliveryAddresses[0].IsPostal,
                "ADR on lines 5-6 is a postal address.");

            Assert.IsTrue(
                card.DeliveryAddresses[0].IsParcel,
                "ADR on lines 5-6 is a parcel address.");

            Assert.AreEqual(
                "6544 Battleford Drive",
                card.DeliveryAddresses[0].Street,
                "ADR on lines 5-6 has a different street address.");

            Assert.AreEqual(
                "Raleigh",
                card.DeliveryAddresses[0].City,
                "ADR on lines 5-6 has a different city.");

            Assert.AreEqual(
                "27613-3502",
                card.DeliveryAddresses[0].PostalCode,
                "ADR on lines 5-6 has a different postal code.");

            Assert.AreEqual(
                "U.S.A.",
                card.DeliveryAddresses[0].Country,
                "ADR on lines 5-6 has a different country.");

            // 07 TEL;TYPE=VOICE,MSG,WORK:+1-919-676-9515
            // 08 TEL;TYPE=FAX,WORK:+1-919-676-9564

            Assert.AreEqual(
                2,
                card.Phones.Count,
                "Two phones are expected at lines 7-8.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL at line 7 is a voice number.");

            Assert.IsTrue(
                card.Phones[0].IsMessagingService,
                "TEL at line 7 is a messaging service.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL at line 7 is a work number.");

            Assert.AreEqual(
                "+1-919-676-9515",
                card.Phones[0].FullNumber,
                "TEL at line 7 has a different number.");

            // 08 TEL;TYPE=FAX,WORK:+1-919-676-9564

            Assert.IsTrue(
                card.Phones[1].IsFax,
                "TEL at line 8 is a fax number.");

            Assert.IsTrue(
                card.Phones[1].IsWork,
                "TEL at line 8 is a work number.");

            Assert.AreEqual(
                "+1-919-676-9564",
                card.Phones[1].FullNumber,
                "TEL at line 8 has a different number.");

            // 09 EMAIL;TYPE=INTERNET,PREF:Frank_Dawson@Lotus.com
            // 10 EMAIL;TYPE=INTERNET:fdawson@earthlink.net

            Assert.AreEqual(
                2,
                card.EmailAddresses.Count,
                "Two email addresses are at lines 9 and 10.");

            Assert.IsTrue(
                card.EmailAddresses[0].EmailType == vCardEmailAddressType.Internet,
                "EMAIL at line 9 is an Internet email address.");

            Assert.IsTrue(
                card.EmailAddresses[0].IsPreferred,
                "EMAIL at line 9 is a preferred email address.");

            Assert.AreEqual(
                "Frank_Dawson@Lotus.com",
                card.EmailAddresses[0].Address,
                "EMAIL at line 9 has a different value.");

            // 10 EMAIL;TYPE=INTERNET:fdawson@earthlink.net

            Assert.AreEqual(
                vCardEmailAddressType.Internet,
                card.EmailAddresses[1].EmailType,
                "EMAIL at line 10 is an Internet email address.");

            Assert.AreEqual(
                "fdawson@earthlink.net",
                card.EmailAddresses[1].Address,
                "EMAIL at line 10 has a different value.");

            // 11 URL:http://home.earthlink.net/~fdawson

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "One URL is located at line 11.");

            Assert.AreEqual(
                "http://home.earthlink.net/~fdawson",
                card.Websites[0].Url,
                "URL at line 11 has a different value.");

        }

        #endregion

        #region [ _parseCard2 ]

        private void _parseCard2(vCard card)
        {

            // 13 BEGIN:vCard
            // 14 VERSION:3.0
            // 15 FN:Tim Howes
            // 16 ORG:Netscape Communications Corp.
            // 17 ADR;TYPE=WORK:;;501 E. Middlefield Rd.;Mountain View;
            // 18  CA; 94043;U.S.A.
            // 19 TEL;TYPE=VOICE,MSG,WORK:+1-415-937-3419
            // 20 TEL;TYPE=FAX,WORK:+1-415-528-4164
            // 21 EMAIL;TYPE=INTERNET:howes@netscape.com
            // 22 END:vCard

            Assert.AreEqual(
                "Tim Howes",
                card.FormattedName,
                "FN at line 15 is different.");

            Assert.AreEqual(
                "Netscape Communications Corp.",
                card.Organization,
                "ORG at line 16 is different.");

            // 17 ADR;TYPE=WORK:;;501 E. Middlefield Rd.;Mountain View;
            // 18  CA; 94043;U.S.A.

            Assert.AreEqual(
                1,
                card.DeliveryAddresses.Count,
                "One ADR located at lines 17-18.");

            Assert.IsTrue(
                card.DeliveryAddresses[0].IsWork,
                "ADR on lines 17-18 is a work address.");

            Assert.AreEqual(
                "501 E. Middlefield Rd.",
                card.DeliveryAddresses[0].Street,
                "ADR on lines 17-18 has a different street.");

            Assert.AreEqual(
                "Mountain View",
                card.DeliveryAddresses[0].City,
                "ADR on lines 17-18 has a different city.");

            Assert.AreEqual(
                "CA",
                card.DeliveryAddresses[0].Region,
                "ADR on lines 17-18 has a different region/state.");

            Assert.AreEqual(
                "94043",
                card.DeliveryAddresses[0].PostalCode,
                "ADR on lines 17-18 has a different postal code.");

            Assert.AreEqual(
                "U.S.A.",
                card.DeliveryAddresses[0].Country,
                "ADR on lines 17-18 has a different country.");

            // 19 TEL;TYPE=VOICE,MSG,WORK:+1-415-937-3419
            // 20 TEL;TYPE=FAX,WORK:+1-415-528-4164

            Assert.AreEqual(
                2,
                card.Phones.Count,
                "Two phones are defined on lines 19 and 20.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL on line 19 is a voice number.");

            Assert.IsTrue(
                card.Phones[0].IsMessagingService,
                "TEL on line 19 is a messaging service number.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL on line 19 is a work number.");

            Assert.AreEqual(
                "+1-415-937-3419",
                card.Phones[0].FullNumber,
                "TEL on line 19 has a different value.");

            // 20 TEL;TYPE=FAX,WORK:+1-415-528-4164

            Assert.IsTrue(
                card.Phones[1].IsFax,
                "TEL on line 20 is a fax number.");

            Assert.IsTrue(
                card.Phones[1].IsWork,
                "TEL on line 20 is a fax number.");

            Assert.AreEqual(
                "+1-415-528-4164",
                card.Phones[1].FullNumber,
                "TEL on line 20 has a different value.");

            // 21 EMAIL;TYPE=INTERNET:howes@netscape.com

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "One email address on line 21.");

            Assert.AreEqual(
                vCardEmailAddressType.Internet,
                card.EmailAddresses[0].EmailType,
                "EMAIL on line 21 is INTERNET.");

            Assert.AreEqual(
                "howes@netscape.com",
                card.EmailAddresses[0].Address,
                "EMAIL on line 21 has a different value.");

        }

        #endregion

    }
}
