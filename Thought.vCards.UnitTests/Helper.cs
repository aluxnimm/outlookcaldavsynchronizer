
using System;
using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;
using Thought.vCards;

namespace Tests
{

    /// <summary>
    ///     Provides common unit test routines.
    /// </summary>
    public static class Helper
    {
 

        // The following functions export a vCard and then re-read it back
        // as a new vCard.  All fields and collections are compared to ensure
        // the full fidelity of the export/import process.

        #region [ CycleStandard ]

        public static void CycleStandard(vCard card)
        {
            CycleStandard21(card);
        }


        #endregion

        #region [ CycleStandard21 ]

        /// <summary>
        ///     Writes a card, then reads it back and compares fields.
        /// </summary>
        public static void CycleStandard21(vCard card)
        {

            if (card == null)
                throw new ArgumentNullException("cycle");

            // Create a memory stream to hold the contents of the card.

            MemoryStream stream = new MemoryStream();

            StreamWriter textWriter = new StreamWriter(stream);

            // Create a standard vCard writer and export the
            // card data to the stream.

            vCardStandardWriter writer = new vCardStandardWriter();
            writer.Write(card, textWriter);
            textWriter.Flush();

            // Reset the stream (back to its beginning), then
            // create a stream reader capable of reading text
            // lines from the stream.

            stream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(stream);

            vCardStandardReader standardReader = new vCardStandardReader();
            vCard reloaded = standardReader.Read(streamReader);

            Equals(card, reloaded);

        }

        #endregion

        // The following functions compare two vCard-related objects.
        
        #region [ Equals(vCard) ]

        public static void Equals(vCard c1, vCard c2)
        {

            // Start by comparing the base fields.

            Assert.AreEqual(
                c1.AdditionalNames,
                c2.AdditionalNames,
                "AdditionalNames does not match.");

            Assert.AreEqual(
                c1.BirthDate,
                c2.BirthDate,
                "BirthDate does not match.");

            Assert.AreEqual(
                c1.DisplayName,
                c2.DisplayName,
                "DisplayName does not match.");

            Assert.AreEqual(
                c1.FamilyName,
                c2.FamilyName,
                "FamilyName does not match.");

            Assert.AreEqual(
                c1.FormattedName,
                c2.FormattedName,
                "FormattedName does not match.");

            Assert.AreEqual(
                c1.Gender,
                c2.Gender,
                "Gender does not match.");

            Assert.AreEqual(
                c1.GivenName,
                c2.GivenName,
                "GivenName does not match.");

            Assert.AreEqual(
                c1.Mailer,
                c2.Mailer,
                "Mailer does not match.");

            Assert.AreEqual(
                c1.NamePrefix,
                c2.NamePrefix,
                "NamePrefix does not match.");

            Assert.AreEqual(
                c1.NameSuffix,
                c2.NameSuffix,
                "NameSuffix does not match.");

            Assert.AreEqual(
                c1.Organization,
                c2.Organization,
                "Organization does not match.");

            Assert.AreEqual(
                c1.ProductId,
                c2.ProductId,
                "ProductId does not match.");

            Assert.AreEqual(
                c1.RevisionDate,
                c2.RevisionDate,
                "RevisionDate does not match.");

            Assert.AreEqual(
                c1.Role,
                c2.Role,
                "Role does not match.");

            Assert.AreEqual(
                c1.TimeZone,
                c2.TimeZone,
                "TimeZone does not match.");

            Assert.AreEqual(
                c1.Title,
                c2.Title,
                "Title does not match.");

            Assert.AreEqual(
                c1.ToString(),
                c2.ToString(),
                "ToString() does not match.");

            Assert.AreEqual(
                c1.UniqueId,
                c2.UniqueId,
                "UniqueId does not match.");

            // Compare collections

            Equals(
                c1.Categories,
                c2.Categories);

            Equals(
                c1.DeliveryAddresses,
                c2.DeliveryAddresses);

            Equals(
                c1.DeliveryLabels,
                c2.DeliveryLabels);

            Equals(
                c1.EmailAddresses,
                c2.EmailAddresses);

            Equals(
                c1.Nicknames,
                c2.Nicknames);

            Equals(
                c1.Notes,
                c2.Notes);

            Equals(
                c1.Phones,
                c2.Phones);

            Equals(
                c1.Photos,
                c2.Photos);

            Equals(
                c1.Sources,
                c2.Sources);

            Equals(
                c1.Websites,
                c2.Websites);

        }

        #endregion

        #region [ Equals(vCardCertificate) ]

        public static void Equals(vCardCertificate c1, vCardCertificate c2)
        {

            Assert.AreEqual(
                c1.KeyType,
                c2.KeyType,
                "The key type of the certificates differ.");

        }

        #endregion

        #region [ Equals(vCardCertificateCollection) ]

        public static void Equals(
            vCardCertificateCollection cc1,
            vCardCertificateCollection cc2)
        {

            Assert.AreEqual(
                cc1.Count,
                cc2.Count,
                "The two certificate collections differ in count.");

            for (int index = 0; index < cc1.Count; index++)
            {
                Equals(cc1[index], cc2[index]);
            }

        }

        #endregion

        #region [ Equals(vCardDeliveryAddress) ]

        public static void Equals(
            vCardDeliveryAddress da1,
            vCardDeliveryAddress da2)
        {

            Assert.AreEqual(
                da1.AddressType,
                da2.AddressType,
                "vCardDeliveryAddress.AddressType differs.");

            Assert.AreEqual(
                da1.City,
                da2.City,
                "vCardDeliveryAddress.City differs.");

            Assert.AreEqual(
                da1.Country,
                da2.Country,
                "vCardDeliveryAddress.Country differs.");

            Assert.AreEqual(
                da1.IsDomestic,
                da2.IsDomestic,
                "vCardDeliveryAddress.IsDomestic differs.");

            Assert.AreEqual(
                da1.IsHome,
                da2.IsHome,
                "vCardDeliveryAddress.IsHome differs.");

            Assert.AreEqual(
                da1.IsInternational,
                da2.IsInternational,
                "vCardDeliveryAddress.IsInternational differs.");

            Assert.AreEqual(
                da1.IsParcel,
                da2.IsParcel,
                "vCardDeliveryAddress.IsParcel differs.");

            Assert.AreEqual(
                da1.IsPostal,
                da2.IsPostal,
                "vCardDeliveryAddress.IsPostal differs.");

            Assert.AreEqual(
                da1.IsWork,
                da2.IsWork,
                "vCardDeliveryAddress.IsWork differs.");

            Assert.AreEqual(
                da1.PostalCode,
                da2.PostalCode,
                "vCardDeliveryAddress.PostalCode differs.");

            Assert.AreEqual(
                da1.Region,
                da2.Region,
                "vCardDeliveryAddress.Region differs.");

            Assert.AreEqual(
                da1.Street,
                da2.Street,
                "vCardDeliveryAddress.Street differs.");

            Assert.AreEqual(
                da1.ToString(),
                da2.ToString(),
                "vCardDeliveryAddress.ToString differs.");

        }

        #endregion

        #region [ Equals(vCardDeliveryAddressCollection) ]

        public static void Equals(
            vCardDeliveryAddressCollection dac1,
            vCardDeliveryAddressCollection dac2)
        {

            Assert.AreEqual(
                dac1.Count,
                dac2.Count,
                "The two delivery address collections differ.");

            for (int index = 0; index < dac1.Count; index++)
            {
                Equals(dac1[index], dac2[index]);
            }

        }

        #endregion

        #region [ Equals(vCardDeliveryLabel) ]

        public static void Equals(
            vCardDeliveryLabel dl1,
            vCardDeliveryLabel dl2)
        {

            Assert.AreEqual(
                dl1.AddressType,
                dl2.AddressType,
                "vCardDeliveryLabel.AddressType differs.");

            Assert.AreEqual(
                dl1.IsDomestic,
                dl2.IsDomestic,
                "vCardDeliveryLabel.IsDomestic differs.");

            Assert.AreEqual(
                dl1.IsHome,
                dl2.IsHome,
                "vCardDeliveryLabel.IsHome differs.");

            Assert.AreEqual(
                dl1.IsInternational,
                dl2.IsInternational,
                "vCardDeliveryLabel.IsInternational differs.");

            Assert.AreEqual(
                dl1.IsParcel,
                dl2.IsParcel,
                "vCardDeliveryLabel.IsParcel differs.");

            Assert.AreEqual(
                dl1.IsPostal,
                dl2.IsPostal,
                "vCardDeliveryLabel.IsPostal differs.");

            Assert.AreEqual(
                dl1.IsWork,
                dl2.IsWork,
                "vCardDeliveryLabel.IsWork differs.");

            Assert.AreEqual(
                dl1.Text,
                dl2.Text,
                "vCardDeliveryLabel.Text differs.");

            Assert.AreEqual(
                dl1.ToString(),
                dl2.ToString(),
                "vCardDeliveryLabel.ToString differs.");

        }

        #endregion

        #region [ Equals(vCardDeliveryLabelCollection) ]

        public static void Equals(
            vCardDeliveryLabelCollection dlc1,
            vCardDeliveryLabelCollection dlc2)
        {

            Assert.AreEqual(
                dlc1.Count,
                dlc2.Count,
                "vCardDeliveryLabelCollection.Count differs.");

            for (int index = 0; index < dlc1.Count; index++)
            {
                Equals(
                    dlc1[index],
                    dlc2[index]);

            }

        }

        #endregion

        #region [ Equals(vCardEmailAddress) ]

        /// <summary>
        ///     Asserts that two vCard email addresses are identical.
        /// </summary>
        public static void Equals(vCardEmailAddress e1, vCardEmailAddress e2)
        {

            Assert.AreEqual(
                e1.Address,
                e2.Address,
                "vCardEmailAddress.Address differs.");

            Assert.AreEqual(
                e1.EmailType,
                e2.EmailType,
                "vCardEmailAddress.EmailType differs.");

            Assert.AreEqual(
                e1.IsPreferred,
                e2.IsPreferred,
                "vCardEmailAddress.IsPreferred differs.");

            Assert.AreEqual(
                e1.ToString(),
                e2.ToString(),
                "vCardEmailAddress.ToString differs.");

        }

        #endregion

        #region [ Equals(vCardEmailAddressCollection) ]

        /// <summary>
        ///     Asserts that two email address collections are
        ///     identical except for ordering of the email addresses.
        /// </summary>
        public static void Equals(
            vCardEmailAddressCollection ec1,
            vCardEmailAddressCollection ec2)
        {

            Assert.AreEqual(
                ec1.Count,
                ec2.Count,
                "The email address collections do not have the same count.");

            for(int index = 0; index < ec1.Count; index++)
            {
                Equals(ec1[index], ec2[index]);
            }

        }

        #endregion

        #region [ Equals(vCardNote) ]

        /// <summary>
        ///     Asserts that two vCard notes are identical.
        /// </summary>
        public static void Equals(vCardNote n1, vCardNote n2)
        {

            Assert.AreEqual(
                n1.Language,
                n2.Language,
                "vCardNote.Language differs.");

            Assert.AreEqual(
                n1.Text,
                n2.Text,
                "vCardNote.Text differs.");

            Assert.AreEqual(
                n1.ToString(),
                n2.ToString(),
                "vCardNote.ToString differs.");

        }

        #endregion

        #region [ Equals(vCardNoteCollection) ]

        public static void Equals(vCardNoteCollection nc1, vCardNoteCollection nc2)
        {

            Assert.AreEqual(
                nc1.Count,
                nc2.Count,
                "The two note collections have a different count.");

            for (int index = 0; index < nc1.Count; index++)
            {
                Equals(nc1[index], nc2[index]);
            }

        }

        #endregion

        #region [ Equals(vCardPhone) ]

        /// <summary>
        ///     Asserts that two vCard phones are identical.
        /// </summary>
        public static void Equals(vCardPhone p1, vCardPhone p2)
        {

            Assert.AreEqual(
                p1.FullNumber,
                p2.FullNumber,
                "vCardPhone.FullNumber differs.");

            Assert.AreEqual(
                p1.IsBBS,
                p2.IsBBS,
                "vCardPhone.IsBBS does not match.");

            Assert.AreEqual(
                p1.IsCar,
                p2.IsCar,
                "vCardPhone.IsCar does not match.");

            Assert.AreEqual(
                p1.IsCellular,
                p2.IsCellular,
                "vCardPhone.IsCellular does not match.");

            Assert.AreEqual(
                p1.IsFax,
                p2.IsFax,
                "vCardPhone.IsFax does not match.");

            Assert.AreEqual(
                p1.IsHome,
                p2.IsHome,
                "vCardPhone.IsHome does not match.");

            Assert.AreEqual(
                p1.IsISDN,
                p2.IsISDN,
                "vCardPhone.IsISDN does not match.");

            Assert.AreEqual(
                p1.IsMessagingService,
                p2.IsMessagingService,
                "vCardPhone.IsMessagingService does not match.");

            Assert.AreEqual(
                p1.IsModem,
                p2.IsModem,
                "vCardPhone.IsModem does not match.");

            Assert.AreEqual(
                p1.IsPager,
                p2.IsPager,
                "vCardPhone.IsPager does not match.");

            Assert.AreEqual(
                p1.IsPreferred,
                p2.IsPreferred,
                "vCardPhone.IsPreferred differs.");

            Assert.AreEqual(
                p1.IsVideo,
                p2.IsVideo,
                "vCardPhone.IsVideo does not match.");

            Assert.AreEqual(
                p1.IsVoice,
                p2.IsVoice,
                "vCardPhone.IsVoice does not match.");

            Assert.AreEqual(
                p1.IsWork,
                p2.IsWork,
                "vCardPhone.IsWork does not match.");

            Assert.AreEqual(
                p1.PhoneType,
                p2.PhoneType,
                "vCardPhone.PhoneType differs.");

            Assert.AreEqual(
                p1.ToString(),
                p2.ToString(),
                "vCardPhone.ToString differs.");

        }

        #endregion

        #region [ Equals(vCardPhoneCollection) ]

        public static void Equals(vCardPhoneCollection pc1, vCardPhoneCollection pc2)
        {

            Assert.AreEqual(
                pc1.Count,
                pc2.Count,
                "The phone collections do not have the same count.");

            for (int index = 0; index < pc1.Count; index++)
            {
                Equals(pc1[index], pc2[index]);
            }

        }

        #endregion

        #region [ Equals(vCardPhoto) ]

        public static void Equals(vCardPhoto p1, vCardPhoto p2)
        {

            Assert.AreEqual(
                p1.IsLoaded,
                p2.IsLoaded,
                "vCardPhoto.IsLoaded differs.");

            Assert.AreEqual(
                p1.ToString(),
                p2.ToString(),
                "vCardPhoto.ToString differs.");

            Assert.AreEqual(
                p1.Url,
                p2.Url,
                "vCardPhoto.Url differs.");

        }

        #endregion

        #region [ Equals(vCardPhotoCollection) ]

        public static void Equals(
            vCardPhotoCollection pc1,
            vCardPhotoCollection pc2)
        {

            Assert.AreEqual(
                pc1.Count,
                pc2.Count,
                "The two photo collections differ in count.");

            for (int index = 0; index < pc1.Count; index++)
            {
                Equals(pc1[index], pc2[index]);
            }

        }

        #endregion

        #region [ Equals(vCardSource) ]

        public static void Equals(vCardSource s1, vCardSource s2)
        {

            Assert.AreEqual(
                s1.Context,
                s2.Context,
                "vCardSource.Context differs.");

            Assert.AreEqual(
                s1.ToString(),
                s2.ToString(),
                "vCardSource.ToString differs.");

            Assert.AreEqual(
                s1.Uri,
                s2.Uri,
                "vCardSource.Uri differs.");

        }

        #endregion

        #region [ Equals(vCardSourceCollection) ]

        public static void Equals(
            vCardSourceCollection sc1,
            vCardSourceCollection sc2)
        {

            Assert.AreEqual(
                sc1.Count,
                sc2.Count,
                "The two source collections differ.");

            for (int index = 0; index < sc1.Count; index++)
            {
                Equals(sc1[index], sc2[index]);
            }
        }

        #endregion

        #region [ Equals(vCardWebSite) ]

        /// <summary>
        ///     Asserts that two web sites are identical.
        /// </summary>
        public static void Equals(vCardWebsite w1, vCardWebsite w2)
        {

            Assert.AreEqual(
                w1.IsPersonalSite,
                w2.IsPersonalSite,
                "vCardWebSite.IsPersonalSite differs.");

            Assert.AreEqual(
                w1.IsWorkSite,
                w2.IsWorkSite,
                "vCardWebSite.IsWorkSite differs.");

            Assert.AreEqual(
                w1.ToString(),
                w2.ToString(),
                "vCardWebSite.ToString() differs.");

            Assert.AreEqual(
                w1.Url,
                w2.Url,
                "vCardWebSite.Url differs.");

            Assert.AreEqual(
                w1.WebsiteType,
                w2.WebsiteType,
                "vCardWebSite.WebSiteType differs.");

        }

        #endregion

        #region [ Equals(vCardWebSiteCollection) ]

        public static void Equals(vCardWebsiteCollection wc1, vCardWebsiteCollection wc2)
        {

            Assert.AreEqual(
                wc1.Count,
                wc2.Count,
                "The web site collections do not have the same count.");

            for (int index = 0; index < wc1.Count; index++)
            {
                Equals(wc1[index], wc2[index]);
            }

        }

        #endregion

        #region [ Equals(StringCollection) ]

        public static void Equals(StringCollection sc1, StringCollection sc2)
        {

            Assert.AreEqual(
                sc1.Count,
                sc2.Count,
                "The two string collections differ in count.");

            for (int index = 0; index < sc1.Count; index++)
            {

                Assert.AreEqual(
                    sc1[index],
                    sc2[index],
                    "The strings differ at index " + index + ".");

            }

        }

        #endregion

    }
}
