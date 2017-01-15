using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;
using System.IO;

namespace Tests.Samples
{
    /// <summary>
    /// Summary description for IntegrationTest
    /// </summary>
    [TestClass]
    public class IntegrationTest
    {
        public IntegrationTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SamplevCardReadAndWriteTestWithEmailTypeFormat()
        {

            vCard card = new vCard();

            card.EmailAddresses.Add(new vCardEmailAddress() { Address = "john@email.com", EmailType = vCardEmailAddressType.Internet, IsPreferred = true, ItemType = ItemType.WORK });



            card.UniqueId = Guid.NewGuid().ToString("N");

            string text = card.ToString();

            vCardStandardWriter writer = new vCardStandardWriter();

            using (StringWriter sw = new StringWriter())
            {

                writer.Write(card, sw);

                sw.Flush();
                text = sw.ToString();
                sw.Close();
            }


            Assert.IsNotNull(text);


            vCardStandardReader reader = new vCardStandardReader();

            using (StringReader sr = new StringReader(text))
            {

                vCard cardFromReader = reader.Read(sr);

                Assert.AreEqual(1, cardFromReader.EmailAddresses.Count);

                var email = cardFromReader.EmailAddresses.First();
                Assert.AreEqual(true, email.IsPreferred);
                Assert.AreEqual(ItemType.WORK, email.ItemType);
                Assert.AreEqual(vCardEmailAddressType.Internet, email.EmailType);
                Assert.AreEqual("john@email.com", email.Address);
            }

        }

        [TestMethod]
        public void SamplevCardReadAndWriteTestWithPhotos()
        {
            string base64Photo = @"/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQY
 GBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYa
 KCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAAR
 CAA3AEgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAA
 AgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkK
 FhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWG
 h4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl
 5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREA
 AgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYk
 NOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOE
 hYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk
 5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwC3KUf7wwaiFvuPDDHrVh0yaiMeWrarK2pw
 wi2KLeFfvuD9KiuY4SmI1JPris3xHq0ek2jGMq1zjcseCeB3OK4+HxZdy3DPcXKwRkbVVEHX
 8a5f3k1dLQ6Y0ktzt4oBGc7efellkYDl1UflXAf23Pf+dbC/lSR8iOTdgA9gccVys012929t
 ctdyXCttZM96hU5vRs29nHqes3FzbqhaW6jwOD84pkSxzwiWE70PQiuJsYksIrYXcjiSWQKk
 PmEjJOOnSu58ORMbFgfugjH1wM05R5epnKNitNbjHIorUnh5orRSdjFm6RzUNyTHDI64DAEj
 NWShzXK/EKa4h0lI4Nw81yGYdgAa6KkeZ2FRTbsjm3uhe6m6wjzmZtssj8nnPT2r0bwl8PbX
 WkQTJkOcMw4K1434SWeS5crkhWz9TXtWjeNL62RUsbEKYQPnKnBx159awrNqVketQgoxba1J
 L/4GPFqUklpKGhjG4Ajlq8o8Z6i+m6rLDYpCXVArMUyQfTNfTcPxBYaJb382nSyGWQxbIsn5
 sZ544FfKPjW6sLjxNqrQvIiPcu+NucZOcfhVRSlruYT9ptJWK/hQf2lrol1OR55kUvCB8qof
 p3r1bw8mLGQej4/QV5n4Njt/7Zi8mV3cI2QVx2+tepaAP9Fn/wCup/kKjEJKyRzu9ncS5XBo
 pNUYxwyMvUDiiiEbo53ubgIrN8QQefp8oVBIVG7Yeje1Le38VlA007YRRn3P0rzjVviBfS3c
 kNrEkEe7ardWx/Ku2VCc37oUW07ot6FaLaSQ3ESnyZTnawwV7YNev6Jqun2nhq6eWKBJVjOD
 gDJxXik+tui20wybdkCvj+Fh1qGXUDrV59ne5ZbZV+VVPDH3FctWk+bU9mnW5Vdnufwq8S6X
 cm6tbqeNwxDqG6bhnp74r5d8R3sVzr+oz2/+rluJHQexY4r0AJpunaN9os3PmF9nQjBxya8+
 uNEfzA0dxEQ3IzxWtKHIr9DKtUU5PubnwzzLr7Z52wk/yFev6EmLSf8A66n+Qryz4c2Mmn6p
 cTXDJtaMKCpz1I/wr1bQmBs5COhkJrLEatHFOVmylrp227+/FFQeIXOFUHqaK0pR905Wzh9Z
 1P8AtdWmZiVUYxyB9MVw2ptJDqLM2MqR09+9FFe7XSVNWN6as7HU+CZI7u9u7C5BZJcSLnse
 9dLF8OWursHS7sQSMeFcZX/61FFeRXk4zdjqi/dOWuGkhlnspHDCCZ4zjoSDg4/nV/wJNC+t
 xW91Ek0UoZGV1yMjoR+VFFdc4r2TXkYSep2euafZ6ftlsrdY95+YLxnFa+iOY9MUHqSTRRXl
 PWKMZtsyNal3Tge1FFFdtKK5UZM//9k=";


            vCard card = new vCard();

            card.EmailAddresses.Add(new vCardEmailAddress() { Address = "john@email.com", EmailType = vCardEmailAddressType.Internet, IsPreferred = true, ItemType = ItemType.WORK });
            card.Photos.Add(new vCardPhoto(base64Photo, true));
            card.UniqueId = Guid.NewGuid().ToString("N");

            string text = card.ToString();

            vCardStandardWriter writer = new vCardStandardWriter();

            using (StringWriter sw = new StringWriter())
            {

                writer.Write(card, sw);

                sw.Flush();
                text = sw.ToString();
                sw.Close();
            }


            Assert.IsNotNull(text);





        }

        [TestMethod]
        public void SamlevCardReadAndWriteTestWithContentFromWikipedia()
        {

            string text = @"BEGIN:VCARD
VERSION:3.0
N:Gump;Forrest
FN:Forrest Gump
ORG:Bubba Gump Shrimp Co.
TITLE:Shrimp Man
PHOTO;VALUE=URL;TYPE=GIF:http://www.example.com/dir_photos/my_photo.gif
TEL;TYPE=WORK,VOICE:(111) 555-1212
TEL;TYPE=HOME,VOICE:(404) 555-1212
ADR;TYPE=WORK;type=pref:;;100 Waters Edge;Baytown;LA;30314;United States of America
EMAIL;TYPE=PREF,INTERNET:forrestgump@example.com
REV:2008-04-24T19:52:43Z
END:VCARD";

            //ADR;TYPE=HOME:;;42 Plantation St.;Baytown;LA;30314;United States of America
//LABEL;TYPE=HOME:42 Plantation St.\nBaytown, LA 30314\nUnited States of America


            vCardStandardReader reader = new vCardStandardReader();

            using (StringReader sr = new StringReader(text))
            {

                vCard cardFromReader = reader.Read(sr);

                Assert.AreEqual(1, cardFromReader.EmailAddresses.Count);

                var email = cardFromReader.EmailAddresses.First();
                Assert.AreEqual(true, email.IsPreferred);
                Assert.AreEqual(ItemType.UNSPECIFIED, email.ItemType);
                Assert.AreEqual(vCardEmailAddressType.Internet, email.EmailType);
                Assert.AreEqual("forrestgump@example.com", email.Address);


                Assert.AreEqual("Shrimp Man", cardFromReader.Title);
                Assert.AreEqual("Forrest", cardFromReader.GivenName);
                Assert.AreEqual("Gump", cardFromReader.FamilyName);


                Assert.AreEqual(2, cardFromReader.Phones.Count);

                var phone404 = cardFromReader.Phones.FirstOrDefault(x => x.FullNumber == "(404) 555-1212");
                var phone111 = cardFromReader.Phones.FirstOrDefault(x => x.FullNumber == "(111) 555-1212");

                Assert.IsNotNull(phone111);
                Assert.IsNotNull(phone404);

                Assert.IsTrue(phone111.IsWork);
                Assert.IsTrue(phone111.IsVoice);

                Assert.IsTrue(phone404.IsVoice);
                Assert.IsTrue(phone404.IsHome);

                Assert.AreEqual(1, cardFromReader.DeliveryAddresses.Count);
                var address = cardFromReader.DeliveryAddresses.First();
                Assert.IsNotNull(address);
                Assert.IsTrue(address.AddressType.Any(a => a == vCardDeliveryAddressTypes.Work), "work address type not found");
                Assert.IsTrue(address.AddressType.Any(a => a == vCardDeliveryAddressTypes.Preferred), "preferred address type not found");


                vCardStandardWriter standardWriter = new vCardStandardWriter();

                using (StringWriter sw = new StringWriter())
                {
                    standardWriter.Write(cardFromReader, sw);

                    sw.Flush();
                    var tempStrign =sw.ToString();

                    Assert.IsNotNull(tempStrign);
                }

            }
            


            //need to add social Profile types as property to the vCard object and reader/writer
            //need to try and add a bunch of properties in my ipad NAB and email me a .vcf file
            //then generate via my parser and try and import the VCF into my ipad
            //look at creating nuGet package for deploying the bin / dll


        }

        [TestMethod]
        public void ShouldReadvCardWithAllCBFieldsFilledOutFromiPhoneNAB()
        {
            string text = @"BEGIN:VCARDVERSION:3.0PRODID:-//Apple Inc.//iOS 6.0.1//ENN:iOS;Nic;;;FN:Nic iOSORG:Ibm;TITLE:Sales Guyitem1.EMAIL;type=INTERNET;type=pref:nic.schlueter@dublabs.comEMAIL;type=INTERNET;type=WORK:nic@work.comEMAIL;type=INTERNET;type=WORK:nic2@work.comEMAIL;type=INTERNET;type=HOME:h@h.comEMAIL;type=INTERNET;type=HOME:y@y.comTEL;type=CELL;type=VOICE;type=pref:(202) 333-4555TEL;type=IPHONE;type=CELL;type=VOICE:(202) 333-4444TEL;type=HOME;type=VOICE:(333) 222-2222TEL;type=WORK;type=VOICE:(809) 555-6666 x444TEL;type=MAIN:(609) 888-7777TEL;type=HOME;type=FAX:(555) 444-4443TEL;type=WORK;type=FAX:33322222222item2.TEL:(999) 777-7999item2.X-ABLabel:personalitem3.ADR;type=HOME;type=pref:;;8230 Boone Blvd;Vinna;VA;22182;United Statesitem3.X-ABADR:usitem4.URL;type=pref:http://facebook.com/max.solenderitem4.X-ABLabel:Profileitem5.URL:www.ibm.comitem5.X-ABLabel:_$!<HomePage>!$_item6.X-MSN:msnnameitem6.X-ABLabel:_$!<Other>!$_item7.X-AIM:aolnameitem7.X-ABLabel:_$!<Other>!$_item8.X-YAHOO:yahoonameitem8.X-ABLabel:_$!<Other>!$_item9.X-JABBER:jabbernameitem9.X-ABLabel:_$!<Other>!$_IMPP;X-SERVICE-TYPE=Skype;type=HOME;type=pref:skype:skypeusernameeeIMPP;X-SERVICE-TYPE=Skype;type=WORK:skype:worksyokeusernameitem10.IMPP;X-SERVICE-TYPE=MSN:msnim:msnnameitem10.X-ABLabel:_$!<Other>!$_item11.IMPP;X-SERVICE-TYPE=AIM:aim:aolnameitem11.X-ABLabel:_$!<Other>!$_item12.IMPP;X-SERVICE-TYPE=GoogleTalk:xmpp:gtalknameitem12.X-ABLabel:_$!<Other>!$_item13.IMPP;X-SERVICE-TYPE=Yahoo:ymsgr:yahoonameitem13.X-ABLabel:_$!<Other>!$_item14.IMPP;X-SERVICE-TYPE=Facebook:xmpp:fbchatnameitem14.X-ABLabel:_$!<Other>!$_item15.IMPP;X-SERVICE-TYPE=Jabber:xmpp:jabbernameitem15.X-ABLabel:_$!<Other>!$_item16.IMPP;X-SERVICE-TYPE=GaduGadu;type=HOME;type=pref:x-apple:jdgaduX-SOCIALPROFILE;type=linkedin;x-user=nicatlinkedin:http://www.linkedin.com/in/nicatlinkedinX-SOCIALPROFILE;type=twitter;x-user=tiffanystone:http://twitter.com/tiffanystoneX-SOCIALPROFILE;type=facebook;x-user=tiffatfacebook:http://www.facebook.com/tiffatfacebookX-SOCIALPROFILE;type=twitter;x-user=gregabedard:http://twitter.com/gregabedardPHOTO;TYPE=JPEG;ENCODING=BASE64:
 /9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQY
 GBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYa
 KCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAAR
 CAA3AEgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAA
 AgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkK
 FhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWG
 h4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl
 5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREA
 AgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYk
 NOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOE
 hYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk
 5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwC3KUf7wwaiFvuPDDHrVh0yaiMeWrarK2pw
 wi2KLeFfvuD9KiuY4SmI1JPris3xHq0ek2jGMq1zjcseCeB3OK4+HxZdy3DPcXKwRkbVVEHX
 8a5f3k1dLQ6Y0ktzt4oBGc7efellkYDl1UflXAf23Pf+dbC/lSR8iOTdgA9gccVys012929t
 ctdyXCttZM96hU5vRs29nHqes3FzbqhaW6jwOD84pkSxzwiWE70PQiuJsYksIrYXcjiSWQKk
 PmEjJOOnSu58ORMbFgfugjH1wM05R5epnKNitNbjHIorUnh5orRSdjFm6RzUNyTHDI64DAEj
 NWShzXK/EKa4h0lI4Nw81yGYdgAa6KkeZ2FRTbsjm3uhe6m6wjzmZtssj8nnPT2r0bwl8PbX
 WkQTJkOcMw4K1434SWeS5crkhWz9TXtWjeNL62RUsbEKYQPnKnBx159awrNqVketQgoxba1J
 L/4GPFqUklpKGhjG4Ajlq8o8Z6i+m6rLDYpCXVArMUyQfTNfTcPxBYaJb382nSyGWQxbIsn5
 sZ544FfKPjW6sLjxNqrQvIiPcu+NucZOcfhVRSlruYT9ptJWK/hQf2lrol1OR55kUvCB8qof
 p3r1bw8mLGQej4/QV5n4Njt/7Zi8mV3cI2QVx2+tepaAP9Fn/wCup/kKjEJKyRzu9ncS5XBo
 pNUYxwyMvUDiiiEbo53ubgIrN8QQefp8oVBIVG7Yeje1Le38VlA007YRRn3P0rzjVviBfS3c
 kNrEkEe7ardWx/Ku2VCc37oUW07ot6FaLaSQ3ESnyZTnawwV7YNev6Jqun2nhq6eWKBJVjOD
 gDJxXik+tui20wybdkCvj+Fh1qGXUDrV59ne5ZbZV+VVPDH3FctWk+bU9mnW5Vdnufwq8S6X
 cm6tbqeNwxDqG6bhnp74r5d8R3sVzr+oz2/+rluJHQexY4r0AJpunaN9os3PmF9nQjBxya8+
 uNEfzA0dxEQ3IzxWtKHIr9DKtUU5PubnwzzLr7Z52wk/yFev6EmLSf8A66n+Qryz4c2Mmn6p
 cTXDJtaMKCpz1I/wr1bQmBs5COhkJrLEatHFOVmylrp227+/FFQeIXOFUHqaK0pR905Wzh9Z
 1P8AtdWmZiVUYxyB9MVw2ptJDqLM2MqR09+9FFe7XSVNWN6as7HU+CZI7u9u7C5BZJcSLnse
 9dLF8OWursHS7sQSMeFcZX/61FFeRXk4zdjqi/dOWuGkhlnspHDCCZ4zjoSDg4/nV/wJNC+t
 xW91Ek0UoZGV1yMjoR+VFFdc4r2TXkYSep2euafZ6ftlsrdY95+YLxnFa+iOY9MUHqSTRRXl
 PWKMZtsyNal3Tge1FFFdtKK5UZM//9k=END:VCARD";

            vCardStandardReader reader = new vCardStandardReader();
            using (StringReader sr = new StringReader(text))
            {

                vCard c = reader.Read(sr);

                Assert.AreEqual(5, c.EmailAddresses.Count);

                CheckEmail(c.EmailAddresses, "nic.schlueter@dublabs.com", ItemType.UNSPECIFIED, vCardEmailAddressType.Internet, true);
                CheckEmail(c.EmailAddresses, "nic@work.com", ItemType.WORK, vCardEmailAddressType.Internet, false);
                CheckEmail(c.EmailAddresses, "nic2@work.com", ItemType.WORK, vCardEmailAddressType.Internet, false);
                CheckEmail(c.EmailAddresses, "h@h.com", ItemType.HOME, vCardEmailAddressType.Internet, false);
                CheckEmail(c.EmailAddresses, "y@y.com", ItemType.HOME, vCardEmailAddressType.Internet, false);




                Assert.AreEqual("Sales Guy", c.Title);
                Assert.AreEqual("Ibm", c.Organization);
                Assert.AreEqual("Nic", c.GivenName);
                Assert.AreEqual("iOS", c.FamilyName);

    

                Assert.AreEqual(8, c.Phones.Count);

                CheckPhone(c.Phones, "(202) 333-4555", vCardPhoneTypes.Preferred | vCardPhoneTypes.Cellular | vCardPhoneTypes.Voice, true);
                CheckPhone(c.Phones, "(202) 333-4444", vCardPhoneTypes.IPhone | vCardPhoneTypes.Cellular | vCardPhoneTypes.Voice, false);
                CheckPhone(c.Phones, "(333) 222-2222", vCardPhoneTypes.Home | vCardPhoneTypes.Voice, false);
                CheckPhone(c.Phones, "(809) 555-6666 x444", vCardPhoneTypes.Work | vCardPhoneTypes.Voice, false);
                CheckPhone(c.Phones, "(609) 888-7777", vCardPhoneTypes.Main, false);
                CheckPhone(c.Phones, "(555) 444-4443", vCardPhoneTypes.Home | vCardPhoneTypes.Fax, false);
                CheckPhone(c.Phones, "33322222222", vCardPhoneTypes.Work | vCardPhoneTypes.Fax, false);
                CheckPhone(c.Phones, "(999) 777-7999", vCardPhoneTypes.Default, false);


                //phones and emails are good
                //need to check the physical address parsing and on down

                CheckAddress(c.DeliveryAddresses, "8230 Boone Blvd", "Vinna", "VA", "22182", "United States", vCardDeliveryAddressTypes.Preferred & vCardDeliveryAddressTypes.Home, true);

                CheckIM(c.IMs, "skypeusernameee", IMServiceType.Skype, ItemType.HOME, true);
                CheckIM(c.IMs, "worksyokeusername", IMServiceType.Skype, ItemType.WORK, false);
                CheckIM(c.IMs, "msnname", IMServiceType.MSN, ItemType.UNSPECIFIED, false);
                CheckIM(c.IMs, "aolname", IMServiceType.AIM, ItemType.UNSPECIFIED, false);
                CheckIM(c.IMs, "gtalkname", IMServiceType.GoogleTalk, ItemType.UNSPECIFIED, false);
                CheckIM(c.IMs, "yahooname", IMServiceType.Yahoo, ItemType.UNSPECIFIED, false);
                CheckIM(c.IMs, "fbchatname", IMServiceType.Facebook, ItemType.UNSPECIFIED, false);
                CheckIM(c.IMs, "jabbername", IMServiceType.Jabber, ItemType.UNSPECIFIED, false);
                CheckIM(c.IMs, "jdgadu", IMServiceType.GaduGadu, ItemType.HOME, true);

                Assert.AreEqual(4, c.SocialProfiles.Count);
                CheckSocialProfile(c.SocialProfiles, "nicatlinkedin", "http://www.linkedin.com/in/nicatlinkedin", SocialProfileServiceType.LinkedIn);
                CheckSocialProfile(c.SocialProfiles, "tiffanystone", "http://twitter.com/tiffanystone", SocialProfileServiceType.Twitter);
                CheckSocialProfile(c.SocialProfiles, "tiffatfacebook", "http://www.facebook.com/tiffatfacebook", SocialProfileServiceType.Facebook);
                CheckSocialProfile(c.SocialProfiles, "gregabedard", "http://twitter.com/gregabedard", SocialProfileServiceType.Twitter);

                Assert.AreEqual(1, c.Photos.Count);
                var photo = c.Photos.First();

                Assert.IsFalse(photo.HasEncodedData, "encoded data should is true");
                var encodedString = photo.EncodedData;

                Assert.IsTrue(string.IsNullOrEmpty(encodedString), "encoded data is empty");

                System.Drawing.Bitmap bitmap = photo.GetBitmap();

                Assert.IsNotNull(bitmap);


                //temp quickly
              vCardStandardWriter writer = new vCardStandardWriter();

                using (StringWriter sw = new StringWriter())
                {

                    writer.Write(c, sw);

                    sw.Flush();
                    text = sw.ToString();
                    sw.Close();
                }


                Assert.IsNotNull(text);
              

            }



        }

        [TestMethod]
        public void ShouldReadvCardWithIMFromEmClient()
        {
            string vCardRequest = @"VERSION:3.0
NAME:Aqib Talib
N:Talib;Aqib;;;
FN:Aqib Talib
ADR;HOME:;;1 Aqib Way;Foxboro;MA;;United States
EMAIL;TYPE=INTERNET;TYPE=HOME:blah@talib.com
IMPP;X-SERVICE-TYPE;TYPE=OTHER::google:aqibtalib@gtalk.com
IMPP:ymsgr:talibonyahoo
IMPP:xmpp:talib_jabber
PRODID:-//eM Client/5.0.19406.0
REV:2014-01-15T21:02:43Z
TEL;CELL:(362) 733-2833
UID:D69D252D-3A87-4EC2-A438-800F0234BC54
END:VCARD";

            vCardStandardReader cardReader = new vCardStandardReader();

            using (StringReader sr = new StringReader(vCardRequest))
            {

                var card = cardReader.Read(sr);


                var im = card.IMs.FirstOrDefault(m => m.ServiceType == IMServiceType.GoogleTalk);

                Assert.IsNotNull(im);

                Assert.AreEqual(IMServiceType.GoogleTalk, im.ServiceType , "service type not set to google talk");
                Assert.AreEqual("aqibtalib@gtalk.com", im.Handle);

                var yahooIM = card.IMs.FirstOrDefault(m => m.ServiceType == IMServiceType.Yahoo);
                Assert.IsNotNull(yahooIM);
                Assert.AreEqual(IMServiceType.Yahoo, yahooIM.ServiceType, "serviceType not set for yahoo");
                Assert.AreEqual("talibonyahoo", yahooIM.Handle);


                var jabberIM = card.IMs.FirstOrDefault(m => m.ServiceType == IMServiceType.Jabber);
                Assert.IsNotNull(jabberIM);
                Assert.AreEqual(IMServiceType.Jabber, jabberIM.ServiceType, "serviceType not set for jabber");
                Assert.AreEqual("talib_jabber", jabberIM.Handle);

            }


        }

        private void CheckSocialProfile(vCardSocialProfileCollection sps, string username, string url, SocialProfileServiceType serviceType)
        {
            if (sps == null || sps.Count == 0)
            {
                Assert.Fail("sps null or empty");
            }

            var sp = sps.FirstOrDefault(x => x.Username == username && x.ServiceType == serviceType);

            Assert.IsNotNull(sp, "no match for socialProfile for " + username + " for serviceType " + serviceType.ToString());


            Assert.AreEqual(url, sp.ProfileUrl);
            Assert.AreEqual(username, sp.Username);
            Assert.AreEqual(serviceType, sp.ServiceType);

        }

        private void CheckIM(vCardIMPPCollection ims, string handle, IMServiceType serviceType, ItemType itemType, bool isPreferred)
        {

            if (ims == null || ims.Count == 0)
            {
                Assert.Fail("ims null or empty");
            }

            var im = ims.FirstOrDefault(x => x.Handle == handle && x.ServiceType == serviceType);

            Assert.IsNotNull(im, "im not matched for handle " + handle + " and servicetype " + serviceType.ToString());
            Assert.AreEqual(itemType,im.ItemType);
            Assert.AreEqual(isPreferred, im.IsPreferred);


        }

        private void CheckAddress(vCardDeliveryAddressCollection addresses, string street, string city, string state, string zip, string country, vCardDeliveryAddressTypes addressTypes, bool isPreferred)
        {
            //there is no street address 2 it is just separated with \n

            if (addresses == null || addresses.Count == 0)
            {
                Assert.Fail("addresses null or empty");
            }

            var a = addresses.FirstOrDefault(x => x.Street == street && x.City == city);

            Assert.IsNotNull(a);

            Assert.AreEqual(state, a.Region);
            Assert.AreEqual(zip, a.PostalCode);
            Assert.AreEqual(country, a.Country);



            foreach(var adr in a.AddressType.Where(x => x != vCardDeliveryAddressTypes.Preferred))
            {
                
                Assert.IsTrue(adr.HasFlag(addressTypes), "address types are not equal");
            }
            
          //  Assert.AreEqual(addressTypes, a.AddressType.);
            Assert.AreEqual(isPreferred, a.IsPreferred);
           Assert.AreEqual(a.IsPreferred, a.AddressType.Any(x => x.HasFlag(vCardDeliveryAddressTypes.Preferred)));
        

        }

        private void CheckPhone(vCardPhoneCollection phones, string value, vCardPhoneTypes types, bool isPreferred)
        {

            if (phones == null || phones.Count == 0)
            {
                Assert.Fail("phones null or empty");
            }

            var p = phones.FirstOrDefault(x => x.FullNumber == value);

            if (p == null)
            {
                Assert.Fail("phone number not found for value " + value);
            }

            Assert.AreEqual(types, p.PhoneType);

            Assert.AreEqual(isPreferred, p.IsPreferred);


            //  types.HasFlag(



        }

        private void CheckEmail(vCardEmailAddressCollection emails, string value, ItemType itemType, vCardEmailAddressType type, bool isPreferred)
        {

            if (emails == null || emails.Count == 0)
            {
                Assert.Fail("emails collection is empty or null");
            }


            var email = emails.FirstOrDefault(x => x.Address.Equals(value, StringComparison.OrdinalIgnoreCase));

            if (email == null)
            {
                Assert.Fail("email value " + value + "  is not found in collection");
            }

            Assert.AreEqual(itemType, email.ItemType);
            Assert.AreEqual(isPreferred, email.IsPreferred);
            Assert.AreEqual(type, email.EmailType);



        }


    }
}
