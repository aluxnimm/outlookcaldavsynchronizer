
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.IO;
using System.Text;

namespace Thought.vCards
{

    /// <summary>
    ///     Implements the standard vCard 2.1 and 3.0 text formats.
    /// </summary>
    public class vCardStandardWriter : vCardWriter
    {

        private bool embedInternetImages;
        private bool embedLocalImages;
        private vCardStandardWriterOptions options;
        private string productId;
        private const string TYPE = "TYPE";

        /// <summary>
        ///     The characters that are escaped per the original
        ///     vCard specification.
        /// </summary>
        private readonly char[] standardEscapedCharacters =
            new char[] { ',', '\\', ';', '\r', '\n' };


        /// <summary>
        ///     The characters that are escaped by Microsoft Outlook.
        /// </summary>
        /// <remarks>
        ///     Microsoft Outlook does not property decode escaped
        ///     commas in values.
        /// </remarks>
        private readonly char[] outlookEscapedCharacters =
            new char[] { '\\', ';', '\r', '\n' };


        /// <summary>
        ///     Creates a new instance of the standard writer.
        /// </summary>
        /// <remarks>
        ///     The standard writer is configured to create vCard
        ///     files in the highest supported version.  This is
        ///     currently version 3.0.
        /// </remarks>
        public vCardStandardWriter()
        {
            this.embedLocalImages = true;
        }


        /// <summary>
        ///     Indicates whether images that reference Internet
        ///     URLs should be embedded in the output.  If not, 
        ///     then a URL is written instead.
        /// </summary>
        public bool EmbedInternetImages
        {
            get
            {
                return this.embedInternetImages;
            }
            set
            {
                this.embedInternetImages = value;
            }
        }


        /// <summary>
        ///     Indicates whether or not references to local
        ///     images should be embedded in the vCard.  If not,
        ///     then a local file reference is written instead.
        /// </summary>
        public bool EmbedLocalImages
        {
            get
            {
                return this.embedLocalImages;
            }
            set
            {
                this.embedLocalImages = value;
            }
        }


        /// <summary>
        ///     Extended options for the vCard writer.
        /// </summary>
        public vCardStandardWriterOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
            }
        }


        /// <summary>
        ///     The product ID to use when writing a vCard.
        /// </summary>
        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = value;
            }
        }


        // The next set of functions generate raw vCard properties
        // from an object in the vCard object model.  Every method
        // has a collection (into which new properties should be
        // placed) and a vCard object (from which the properties
        // should be generated).

        #region [ BuildProperties ]

        /// <summary>
        ///     Builds a collection of standard properties based on
        ///     the specified vCard.
        /// </summary>
        /// <returns>
        ///     A <see cref="vCardPropertyCollection"/> that contains all
        ///     properties for the current vCard, including the header
        ///     and footer properties.
        /// </returns>
        /// <seealso cref="vCard"/>
        /// <seealso cref="vCardProperty"/>
        private vCardPropertyCollection BuildProperties(
            vCard card)
        {

            vCardPropertyCollection properties =
                new vCardPropertyCollection();

            // The BEGIN:VCARD line marks the beginning of
            // the vCard contents.  Later it will end with END:VCARD.
            // See section 2.1.1 of RFC 2426.

            properties.Add(new vCardProperty("BEGIN", "VCARD"));
            properties.Add(new vCardProperty("VERSION", "3.0"));
            BuildProperties_NAME(
                properties,
                card);

            BuildProperties_SOURCE(
                properties,
                card);

            BuildProperties_N(
                properties,
                card);

            BuildProperties_FN(
                properties,
                card);

            BuildProperties_ADR(
                properties,
                card);

            BuildProperties_BDAY(
                properties,
                card);

            BuildProperties_CATEGORIES(
                properties,
                card);

            BuildProperties_CLASS(
                properties,
                card);

            BuildProperties_EMAIL(
                properties,
                card);

            BuildProperties_GEO(
                properties,
                card);

            BuildProperties_IMPP(properties, card);

            BuildProperties_KEY(
                properties,
                card);

            BuildProperties_LABEL(
                properties,
                card);

            BuildProperties_MAILER(
                properties,
                card);

            BuildProperties_NICKNAME(
                properties,
                card);

            BuildProperties_NOTE(
                properties,
                card);

            BuildProperties_ORG(
                properties,
                card);

            BuildProperties_PHOTO(
                properties,
                card);

            BuildProperties_PRODID(
                properties,
                card);

            BuildProperties_REV(
                properties,
                card);

            BuildProperties_ROLE(
                properties,
                card);

            BuildProperties_TEL(
                properties,
                card);

            BuildProperties_TITLE(
                properties,
                card);

            BuildProperties_TZ(
                properties,
                card);

            BuildProperties_UID(
                properties,
                card);

            BuildProperties_URL(
                properties,
                card);

            BuildProperties_XSOCIALPROFILE(properties, card);

            BuildProperties_X_WAB_GENDER(
                properties,
                card);

            // The end of the vCard is marked with an END:VCARD.

            properties.Add(new vCardProperty("END", "VCARD"));
            return properties;

        }

        #endregion

        #region [ BuildProperties_ADR ]

        /// <summary>
        ///     Builds ADR properties.
        /// </summary>
        private void BuildProperties_ADR(
            vCardPropertyCollection properties,
            vCard card)
        {

            foreach (vCardDeliveryAddress address in card.DeliveryAddresses)
            {

                // Do not generate a postal address (ADR) property
                // if the entire address is blank.

                if (
                    (!string.IsNullOrEmpty(address.City)) ||
                    (!string.IsNullOrEmpty(address.Country)) ||
                    (!string.IsNullOrEmpty(address.PostalCode)) ||
                    (!string.IsNullOrEmpty(address.Region)) ||
                    (!string.IsNullOrEmpty(address.Street)))
                {

                    // The ADR property contains the following
                    // subvalues in order.  All are required:
                    //
                    //   - Post office box
                    //   - Extended address
                    //   - Street address
                    //   - Locality (e.g. city)
                    //   - Region (e.g. province or state)
                    //   - Postal code (e.g. ZIP code)
                    //   - Country name

                    vCardValueCollection values = new vCardValueCollection(';');

                    values.Add(string.Empty);
                    values.Add(string.Empty);
                    values.Add(!string.IsNullOrEmpty(address.Street) ? address.Street.Replace("\r\n", "\n") : string.Empty);
                    values.Add(address.City);
                    values.Add(address.Region);
                    values.Add(address.PostalCode);
                    values.Add(address.Country);

                    vCardProperty property =
                        new vCardProperty("ADR", values);

                    if (address.IsDomestic)
                        property.Subproperties.Add(TYPE, "DOM");

                    if (address.IsInternational)
                        property.Subproperties.Add(TYPE, "INTL");

                    if (address.IsParcel)
                        property.Subproperties.Add(TYPE, "PARCEL");

                    if (address.IsPostal)
                        property.Subproperties.Add(TYPE, "POSTAL");

                    if (address.IsHome)
                        property.Subproperties.Add(TYPE, "HOME");

                    if (address.IsWork)
                        property.Subproperties.Add(TYPE, "WORK");

                    if (address.IsPreferred)
                    {
                        property.Subproperties.Add(TYPE, "PREF");
                    }

                    properties.Add(property);

                }

            }

        }

        #endregion

        #region [ BuildProperties_BDAY ]

        /// <summary>
        ///     Builds the BDAY property.
        /// </summary>
        private void BuildProperties_BDAY(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The BDAY property indicates the birthdate
            // of the person.  The output format here is based on
            // Microsoft Outlook, which writes the date as YYYMMDD.
            // FIXES DateFormat with ToString

            if (card.BirthDate.HasValue)
            {

                vCardProperty property =
                    new vCardProperty("BDAY", card.BirthDate.Value.ToString("yyyy-MM-dd"));

                properties.Add(property);
            }
        }

        #endregion

        #region [ BuildProperties_CATEGORIES ]

        private void BuildProperties_CATEGORIES(
            vCardPropertyCollection properties,
            vCard card)
        {

            if (card.Categories.Count > 0)
            {

                vCardValueCollection values = new vCardValueCollection(',');

                foreach (string category in card.Categories)
                {

                    if (!string.IsNullOrEmpty(category))
                        values.Add(category);
                }

                properties.Add(
                    new vCardProperty("CATEGORIES", values));

            }

        }

        #endregion

        #region [ BuildProperties_CLASS ]

        private void BuildProperties_CLASS(
            vCardPropertyCollection properties,
            vCard card)
        {

            vCardProperty property = new vCardProperty("CLASS");

            switch (card.AccessClassification)
            {

                case vCardAccessClassification.Unknown:
                    // No value is written.
                    return;

                case vCardAccessClassification.Confidential:
                    property.Value = "CONFIDENTIAL";
                    break;

                case vCardAccessClassification.Private:
                    property.Value = "PRIVATE";
                    break;

                case vCardAccessClassification.Public:
                    property.Value = "PUBLIC";
                    break;

                default:
                    throw new NotSupportedException();

            }

            properties.Add(property);

        }

        #endregion

        #region [ BuildProperties_EMAIL ]

        /// <summary>
        ///     Builds EMAIL properties.
        /// </summary>
        private void BuildProperties_EMAIL(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The EMAIL property contains an electronic
            // mail address for the purpose.  A vCard may contain
            // as many email addresses as needed.  The format also
            // supports various vendors, such as CompuServe addresses
            // and Internet SMTP addresses.
            //
            // EMAIL;INTERNET:support@fairmetric.com

            foreach (vCardEmailAddress emailAddress in card.EmailAddresses)
            {

                vCardProperty property = new vCardProperty();
                property.Name = "EMAIL";
                property.Value = emailAddress.Address;

                if (emailAddress.IsPreferred)
                {
                    property.Subproperties.Add(TYPE, "PREF");
                }

                switch (emailAddress.EmailType)
                {

                    case vCardEmailAddressType.Internet:
                        property.Subproperties.Add(TYPE, "INTERNET");
                        break;

                    case vCardEmailAddressType.AOL:
                        property.Subproperties.Add(TYPE, "AOL");
                        break;

                    case vCardEmailAddressType.AppleLink:
                        property.Subproperties.Add(TYPE, "AppleLink");
                        break;

                    case vCardEmailAddressType.AttMail:
                        property.Subproperties.Add(TYPE, "ATTMail");
                        break;

                    case vCardEmailAddressType.CompuServe:
                        property.Subproperties.Add(TYPE, "CIS");
                        break;

                    case vCardEmailAddressType.eWorld:
                        property.Subproperties.Add(TYPE, "eWorld");
                        break;

                    case vCardEmailAddressType.IBMMail:
                        property.Subproperties.Add(TYPE, "IBMMail");
                        break;

                    case vCardEmailAddressType.MCIMail:
                        property.Subproperties.Add(TYPE, "MCIMail");
                        break;

                    case vCardEmailAddressType.PowerShare:
                        property.Subproperties.Add(TYPE, "POWERSHARE");
                        break;

                    case vCardEmailAddressType.Prodigy:
                        property.Subproperties.Add(TYPE, "PRODIGY");
                        break;

                    case vCardEmailAddressType.Telex:
                        property.Subproperties.Add(TYPE, "TLX");
                        break;

                    case vCardEmailAddressType.X400:
                        property.Subproperties.Add(TYPE, "X400");
                        break;

                    default:
                        property.Subproperties.Add(TYPE, "INTERNET");
                        break;

                }

                switch (emailAddress.ItemType)
                {
                    case ItemType.UNSPECIFIED:
                        //do nothing
                        break;
                    case ItemType.HOME:
                        property.Subproperties.Add(TYPE, ItemType.HOME.ToString());
                        break;
                    case ItemType.WORK:
                        property.Subproperties.Add(TYPE, ItemType.WORK.ToString());
                        break;

                    default:

                        break;
                }

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_FN ]

        private void BuildProperties_FN(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The FN property indicates the formatted 
            // name of the person.  This can be something
            // like "John Smith".

            if (!string.IsNullOrEmpty(card.FormattedName))
            {

                vCardProperty property =
                    new vCardProperty("FN", card.FormattedName);

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_GEO ]

        /// <summary>
        ///     Builds the GEO property.
        /// </summary>
        private void BuildProperties_GEO(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The GEO properties contains the latitude and
            // longitude of the person or company of the vCard.

            if (card.Latitude.HasValue && card.Longitude.HasValue)
            {

                vCardProperty property = new vCardProperty();

                property.Name = "GEO";
                property.Value =
                    card.Latitude.ToString() + ";" + card.Longitude.ToString();

                properties.Add(property);

            }

        }

    #endregion


    private void BuildProperties_IMPP(vCardPropertyCollection properties, vCard card)
    {

      // adding support for IMPP (IM handles) in the vCard
      //iOS outputs this => IMPP;X-SERVICE-TYPE=Skype;type=HOME;type=pref:skype:skypeusernameee



      foreach (var im in card.IMs)
      {

        vCardProperty property = new vCardProperty();
        property.Name = "IMPP";

        string prefix = IMTypeUtils.GetIMTypePropertyPrefix(im.ServiceType);
        string suffix = IMTypeUtils.GetIMTypePropertySuffix(im.ServiceType);

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix))
        {
          property.Subproperties.Add("X-SERVICE-TYPE", prefix);
          property.Value = string.Concat(suffix, ":", im.Handle);
        }
        else
        {
          property.Value = im.Handle;
        }


        if (im.IsPreferred)
        {
          property.Subproperties.Add(TYPE, "PREF");
        }

        switch (im.ItemType)
        {

          case ItemType.HOME:
            property.Subproperties.Add(TYPE, ItemType.HOME.ToString());
            break;
          case ItemType.WORK:
            property.Subproperties.Add(TYPE, ItemType.WORK.ToString());
            break;

          case ItemType.UNSPECIFIED:
          default:
            property.Subproperties.Add(TYPE, "OTHER");
            break;
        }

        properties.Add(property);

        if (im.ServiceType == IMServiceType.AIM)
        {
          var propertyXAim = new vCardProperty("X-AIM", im.Handle);
          properties.Add(propertyXAim);
        }
      }

    }


    #region [ BuildProperties_KEY ]

    /// <summary>
    ///     Builds KEY properties.
    /// </summary>
    private void BuildProperties_KEY(
            vCardPropertyCollection properties,
            vCard card)
        {

            // A KEY field contains an embedded security certificate.

            foreach (vCardCertificate certificate in card.Certificates)
            {

                vCardProperty property = new vCardProperty();

                property.Name = "KEY";
                property.Value = certificate.Data;
                property.Subproperties.Add(TYPE, certificate.KeyType);

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_LABEL ]

        private void BuildProperties_LABEL(
            vCardPropertyCollection properties,
            vCard card)
        {

            foreach (vCardDeliveryLabel label in card.DeliveryLabels)
            {

                if (label.Text.Length > 0)
                {

                    vCardProperty property = new vCardProperty("LABEL", label.Text);

                    if (label.IsDomestic)
                        property.Subproperties.Add(TYPE, "DOM");

                    if (label.IsInternational)
                        property.Subproperties.Add(TYPE, "INTL");

                    if (label.IsParcel)
                        property.Subproperties.Add(TYPE, "PARCEL");

                    if (label.IsPostal)
                        property.Subproperties.Add(TYPE, "POSTAL");

                    if (label.IsHome)
                        property.Subproperties.Add(TYPE, "HOME");

                    if (label.IsWork)
                        property.Subproperties.Add(TYPE, "WORK");

                    properties.Add(property);


                }

            }

        }

        #endregion

        #region [ BuildProperties_MAILER ]

        /// <summary>
        ///     Builds the MAILER property.
        /// </summary>
        private void BuildProperties_MAILER(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The MAILER property indicates the software that
            // generated the vCard.  See section 2.4.3 of the
            // vCard 2.1 specification.  Support is not widespread.

            if (!string.IsNullOrEmpty(card.Mailer))
            {

                vCardProperty property =
                    new vCardProperty("MAILER", card.Mailer);

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_N ]

        private void BuildProperties_N(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The property has the following components: Family Name,
            // Given Name, Additional Names, Name Prefix, and Name
            // Suffix.  Example:
            //
            //   N:Pinch;David
            //   N:Pinch;David;John
            //
            // The N property is required (see section 3.1.2 of RFC 2426).

            vCardValueCollection values = new vCardValueCollection(';');
            values.Add(card.FamilyName);
            values.Add(card.GivenName);
            values.Add(card.AdditionalNames);
            values.Add(card.NamePrefix);
            values.Add(card.NameSuffix);

            vCardProperty property = new vCardProperty("N", values);

            properties.Add(property);

        }

        #endregion

        #region [ BuildProperties_NAME ]

        private void BuildProperties_NAME(
            vCardPropertyCollection properties,
            vCard card)
        {

            if (!string.IsNullOrEmpty(card.DisplayName))
            {

                vCardProperty property =
                    new vCardProperty("NAME", card.DisplayName);

                properties.Add(property);
            }

        }

        #endregion

        #region [ BuildProperties_NICKNAME ]

        /// <summary>
        ///     Builds the NICKNAME property.
        /// </summary>
        private void BuildProperties_NICKNAME(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The NICKNAME property specifies the familiar name
            // of the person, such as Jim.  This is defined in
            // section 3.1.3 of RFC 2426.  Multiple names can
            // be listed, separated by commas.

            if (card.Nicknames.Count > 0)
            {

                // A NICKNAME property is a comma-separated
                // list of values.  Create a value list and
                // add the nicknames collection to it.

                vCardValueCollection values = new vCardValueCollection(',');
                values.Add(card.Nicknames);

                // Create the new properties with each name separated
                // by a comma.

                vCardProperty property =
                    new vCardProperty("NICKNAME", values);

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_NOTE ]

        /// <summary>
        ///     Builds the NOTE property.
        /// </summary>
        private void BuildProperties_NOTE(
            vCardPropertyCollection properties,
            vCard card)
        {

            foreach (vCardNote note in card.Notes)
            {

                if (!string.IsNullOrEmpty(note.Text))
                {

                    vCardProperty property = new vCardProperty();

                    property.Name = "NOTE";
                    property.Value = note.Text.Replace("\r\n", "\n");

                    if (!string.IsNullOrEmpty(note.Language))
                    {
                        property.Subproperties.Add("language", note.Language);
                    }

                    properties.Add(property);

                }

            }

        }

        #endregion

        #region [ BuildProperties_ORG ]

        /// <summary>
        ///     Builds the ORG property.
        /// </summary>
        private void BuildProperties_ORG(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The ORG property specifies the name of the
            // person's company or organization. Example:
            //
            // ORG:FairMetric LLC

            if (!string.IsNullOrEmpty(card.Organization))
            {

                vCardProperty property;
           
                // Add department also
                if (!string.IsNullOrEmpty(card.Department))
                {
                    vCardValueCollection values = new vCardValueCollection(';');
                    values.Add(card.Organization);
                    values.Add(card.Department);
                    property = new vCardProperty("ORG", values);
                }
                else
                {
                    property = new vCardProperty("ORG", card.Organization);
                }

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_PHOTO ]

        private void BuildProperties_PHOTO(
            vCardPropertyCollection properties,
            vCard card)
        {

            foreach (vCardPhoto photo in card.Photos)
            {

              if (photo.Url == null)
              {

                // This photo does not have a URL associated
                // with it.  Therefore a property can be
                // generated only if the image data is loaded.
                // Otherwise there is not enough information.

                vCardProperty property = null;

                if (photo.IsLoaded)
                {
                  property = new vCardProperty("PHOTO", photo.GetBytes());
                }
                else if (photo.HasEncodedData)
                {
                  property = new vCardProperty("PHOTO", photo.EncodedData);
                }

                if (property != null)
                {
                  property.Subproperties.Add("TYPE", "JPEG");
                  properties.Add(property);
                }
                
              }
              else
              {

                // This photo has a URL associated with it.  The
                // PHOTO property can either be linked as an image
                // or embedded, if desired.

                bool doEmbedded =
                  photo.Url.IsFile ? this.embedLocalImages : this.embedInternetImages;

                if (doEmbedded)
                {

                  // According to the settings of the card writer,
                  // this linked image should be embedded into the
                  // vCard data.  Attempt to fetch the data.

                  try
                  {
                    photo.Fetch();
                  }
                  catch
                  {

                    // An error was encountered.  The image can
                    // still be written as a link, however.

                    doEmbedded = false;
                  }

                }

                // At this point, doEmbedded is true only if (a) the
                // writer was configured to embed the image, and (b)
                // the image was successfully downloaded.

                if (doEmbedded)
                {
                  properties.Add(
                    new vCardProperty("PHOTO", photo.GetBytes()));
                }
                else
                {

                  vCardProperty uriPhotoProperty =
                    new vCardProperty("PHOTO");

                  // Set the VALUE property to indicate that
                  // the data for the photo is a URI.

                  uriPhotoProperty.Subproperties.Add("VALUE", "URI");
                  uriPhotoProperty.Value = photo.Url.ToString();

                  properties.Add(uriPhotoProperty);
                }

              }
            }
        }

        #endregion

        #region [ BuildProperties_PRODID ]

        /// <summary>
        ///     Builds PRODID properties.
        /// </summary>
        private void BuildProperties_PRODID(
            vCardPropertyCollection properties,
            vCard card)
        {

            if (!string.IsNullOrEmpty(card.ProductId))
            {
                vCardProperty property = new vCardProperty();
                property.Name = "PRODID";
                property.Value = card.ProductId;
                properties.Add(property);
            }

        }

        #endregion

        #region [ BuildProperties_REV ]

        /// <summary>
        ///     Builds the REV property.
        /// </summary>
        private void BuildProperties_REV(
            vCardPropertyCollection properties,
            vCard card)
        {

            if (card.RevisionDate.HasValue)
            {

                vCardProperty property =
                    new vCardProperty("REV", card.RevisionDate.Value.ToString("s") + "Z");

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_ROLE ]

        /// <summary>
        ///     Builds the ROLE property.
        /// </summary>
        private void BuildProperties_ROLE(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The ROLE property identifies the role of
            // the person at his/her organization.

            if (!string.IsNullOrEmpty(card.Role))
            {

                vCardProperty property =
                    new vCardProperty("ROLE", card.Role);

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_SOURCE ]

        /// <summary>
        ///     Builds SOURCE properties.
        /// </summary>
        private void BuildProperties_SOURCE(
            vCardPropertyCollection properties,
            vCard card)
        {

            foreach (vCardSource source in card.Sources)
            {

                vCardProperty property = new vCardProperty();

                property.Name = "SOURCE";
                property.Value = source.Uri.ToString();

                if (!string.IsNullOrEmpty(source.Context))
                    property.Subproperties.Add("CONTEXT", source.Context);

                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_TEL ]

        /// <summary>
        ///     Builds TEL properties.
        /// </summary>
        private void BuildProperties_TEL(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The TEL property indicates a telephone number of
            // the person (including non-voice numbers like fax
            // and BBS numbers).
            //
            // TEL;VOICE;WORK:1-800-929-5805

            foreach (vCardPhone phone in card.Phones)
            {

                // A telephone entry has the property name TEL and
                // can have zero or more subproperties like FAX
                // or HOME.  Examples:
                //
                //   TEL;HOME:+1-612-555-1212
                //   TEL;FAX;HOME:+1-612-555-1212

                vCardProperty property = new vCardProperty();

                property.Name = "TEL";

                if (phone.IsBBS)
                  property.Subproperties.Add(TYPE, "BBS");

                if (phone.IsCar)
                    property.Subproperties.Add(TYPE, "CAR");

                if (phone.IsCellular)
                    property.Subproperties.Add(TYPE, "CELL");

                if (phone.IsFax)
                {
                    if (!phone.IsHome && !phone.IsWork)
                    {
                        property.Subproperties.Add(TYPE, "OTHER");
                    }
                    property.Subproperties.Add(TYPE, "FAX");
                }

                if (phone.IsHome)
                    property.Subproperties.Add(TYPE, "HOME");

                if (phone.IsISDN)
                    property.Subproperties.Add(TYPE, "ISDN");

                if (phone.IsMessagingService)
                    property.Subproperties.Add(TYPE, "MSG");

                if (phone.IsModem)
                    property.Subproperties.Add(TYPE, "MODEM");

                if (phone.IsPager)
                    property.Subproperties.Add(TYPE, "PAGER");

                if (phone.IsPreferred)
                    property.Subproperties.Add(TYPE, "PREF");

                if (phone.IsVideo)
                    property.Subproperties.Add(TYPE, "VIDEO");

                if (phone.IsVoice)
                {
                    if (!phone.IsHome && !phone.IsWork)
                    {
                        property.Subproperties.Add(TYPE, "OTHER");
                    }
                    property.Subproperties.Add(TYPE, "VOICE");
                }

                if (phone.IsWork)
                    property.Subproperties.Add(TYPE, "WORK");

                if (phone.IsiPhone)
                {
                    property.Subproperties.Add(TYPE, "IPHONE");
                }
                if (phone.IsMain)
                {
                    property.Subproperties.Add(TYPE, "MAIN");
                }

                property.Value = phone.FullNumber;
                properties.Add(property);

            }

        }

        #endregion

        #region [ BuildProperties_TITLE ]

        private void BuildProperties_TITLE(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The TITLE property specifies the job title of 
            // the person.  Example:
            //
            // TITLE:Systems Analyst
            // TITLE:President

            if (!string.IsNullOrEmpty(card.Title))
            {
                vCardProperty property =
                    new vCardProperty("TITLE", card.Title);

                properties.Add(property);
            }

        }

        #endregion

        #region [ BuildProperties_TZ ]

        private void BuildProperties_TZ(
            vCardPropertyCollection properties,
            vCard card)
        {

            if (!string.IsNullOrEmpty(card.TimeZone))
            {
                properties.Add(new vCardProperty("TZ", card.TimeZone));
            }

        }

        #endregion

        #region [ BuildProperties_UID ]

        private void BuildProperties_UID(
            vCardPropertyCollection properties,
            vCard card)
        {

            if (!string.IsNullOrEmpty(card.UniqueId))
            {
                vCardProperty property = new vCardProperty();
                property.Name = "UID";
                property.Value = card.UniqueId;
                properties.Add(property);
            }

        }

        #endregion

        #region [ BuildProperties_URL ]

        private void BuildProperties_URL(
            vCardPropertyCollection properties,
            vCard card)
        {

            foreach (vCardWebsite webSite in card.Websites)
            {

                if (!string.IsNullOrEmpty(webSite.Url))
                {
                    vCardProperty property =
                        new vCardProperty("URL", webSite.Url.ToString());

                    if (webSite.IsWorkSite)
                        property.Subproperties.Add(TYPE, "WORK");
                    // Add Subproperty for HOME aswell
                    if (webSite.IsPersonalSite)
                        property.Subproperties.Add(TYPE, "HOME");

                    properties.Add(property);
                }

            }

        }

        #endregion


        private void BuildProperties_XSOCIALPROFILE(vCardPropertyCollection properties, vCard card)
        {

            // adding support for X-SOCIALPROFILE) in the vCard


            foreach (var sp in card.SocialProfiles)
            {

                vCardProperty property = new vCardProperty();
                property.Name = "X-SOCIALPROFILE";

                string propertyType = SocialProfileTypeUtils.GetSocialProfileServicePropertyType(sp.ServiceType);

                property.Subproperties.Add("TYPE", propertyType);
                property.Subproperties.Add("X-USER", sp.Username);
                property.Value = sp.ProfileUrl;

                properties.Add(property);

            }

        }


        #region [ BuildProperties_X_WAB_GENDER ]

        private void BuildProperties_X_WAB_GENDER(
            vCardPropertyCollection properties,
            vCard card)
        {

            // The X-WAB-GENDER property is an extended (custom)
            // property supported by Microsoft Outlook.

            switch (card.Gender)
            {
                case vCardGender.Female:
                    properties.Add(new vCardProperty("X-WAB-GENDER", "1"));
                    break;

                case vCardGender.Male:
                    properties.Add(new vCardProperty("X-WAB-GENDER", "2"));
                    break;

            }

        }

        #endregion

        // The next set of functions translate raw values into
        // various string encodings.  A vCard file is a text file
        // with a defined format; values that break the format (such
        // as binary values or strings with ASCII control characters)
        // must be encoded.

        #region [ EncodeBase64(byte) ]

        /// <summary>
        ///     Converts a byte to a BASE64 string.
        /// </summary>
        public static string EncodeBase64(byte value)
        {
            return Convert.ToBase64String(new byte[] { value });
        }

        #endregion

        #region [ EncodeBase64(byte[]) ]

        /// <summary>
        ///     Converts a byte array to a BASE64 string.
        /// </summary>
        public static string EncodeBase64(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        #endregion

        #region [ EncodeBase64(int) ]

        /// <summary>
        ///     Converts an integer to a BASE64 string.
        /// </summary>
        public static string EncodeBase64(int value)
        {

            byte[] buffer = new byte[4];

            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            return Convert.ToBase64String(buffer);
        }

        #endregion

        #region [ EncodeEscaped(string) ]

        /// <summary>
        ///     Encodes a string using simple escape codes.
        /// </summary>
        public string EncodeEscaped(string value)
        {

            if (
                (this.options & vCardStandardWriterOptions.IgnoreCommas) ==
                    vCardStandardWriterOptions.IgnoreCommas)
            {
                return EncodeEscaped(value, outlookEscapedCharacters);
            }
            else
            {
                return EncodeEscaped(value, standardEscapedCharacters);
            }
        }

        #endregion

        #region [ EncodeEscaped(string, char[]) ]

        /// <summary>
        ///     Encodes a character array using simple escape sequences.
        /// </summary>
        public static string EncodeEscaped(
            string value,
            char[] escaped)
        {

            if (escaped == null)
                throw new ArgumentNullException("escaped");

            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder buffer = new StringBuilder();

            int startIndex = 0;

            do
            {

                // Get the index of the next character
                // to be escaped (e.g. the next semicolon).

                int nextIndex = value.IndexOfAny(escaped, startIndex);

                if (nextIndex == -1)
                {
                    // No more characters need to be escaped.
                    // Any characters between the start index
                    // and the end of the string can be copied
                    // to the buffer.

                    buffer.Append(
                        value,
                        startIndex,
                        value.Length - startIndex);

                    break;

                }
                else
                {

                    char replacement;
                    switch (value[nextIndex])
                    {
                        case '\n':
                            replacement = 'n';
                            break;

                        case '\r':
                            replacement = 'r';
                            break;

                        default:
                            replacement = value[nextIndex];
                            break;

                    }

                    buffer.Append(
                        value,
                        startIndex,
                        nextIndex - startIndex);

                    buffer.Append('\\');
                    buffer.Append(replacement);

                    startIndex = nextIndex + 1;

                }

            } while (startIndex < value.Length);

            return buffer.ToString();

            // The following must be encoded:
            //
            // Backslash (\\)
            // Colon (\:)
            // Semicolon (\;)

        }

        #endregion

        #region [ EncodeQuotedPrintable ]

        /// <summary>
        ///     Converts a string to quoted-printable format.
        /// </summary>
        /// <param name="value">
        ///     The value to encode in Quoted Printable format.
        /// </param>
        /// <returns>
        ///     The value encoded in Quoted Printable format.
        /// </returns>
        public static string EncodeQuotedPrintable(string value)
        {

            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder builder = new StringBuilder();

            foreach (char c in value)
            {

                int v = (int)c;

                // The following are not required to be encoded:
                //
                // - Tab (ASCII 9)
                // - Space (ASCII 32)
                // - Characters 33 to 126, except for the equal sign (61).

                if (
                    (v == 9) ||
                    ((v >= 32) && (v <= 60)) ||
                    ((v >= 62) && (v <= 126)))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append('=');
                    builder.Append(v.ToString("X2"));
                }

            }

            char lastChar = builder[builder.Length - 1];
            if (char.IsWhiteSpace(lastChar))
            {
                builder.Remove(builder.Length - 1, 1);
                builder.Append('=');
                builder.Append(((int)lastChar).ToString("X2"));
            }

            return builder.ToString();

        }

        #endregion

        /// <summary>
        ///     Returns property encoded into a standard vCard NAME:VALUE format.
        /// </summary>
        public string EncodeProperty(vCardProperty property)
        {

            if (property == null)
                throw new ArgumentNullException("property");

            if (string.IsNullOrEmpty(property.Name))
                throw new ArgumentException();

            StringBuilder builder = new StringBuilder();

            builder.Append(property.Name);

            foreach (vCardSubproperty subproperty in property.Subproperties)
            {
                builder.Append(';');
                builder.Append(subproperty.Name);

                if (!string.IsNullOrEmpty(subproperty.Value))
                {
                    builder.Append('=');
                    builder.Append(subproperty.Value);
                }
            }

            // The property name and all subproperties have been
            // written to the string builder (the colon separator
            // has not been written).  The next step is to write
            // the value.  Depending on the type of value and any
            // characters in the value, it may be necessary to
            // use an non-default encoding.  For example, byte arrays
            // are written encoded in BASE64.

            if (property.Value == null)
            {
                builder.Append(':');
            }
            else
            {

                Type valueType = property.Value.GetType();

                if (valueType == typeof(byte[]))
                {

                    // A byte array should be encoded in BASE64 format.

                    builder.Append(";ENCODING=b:");
                    builder.Append(EncodeBase64((byte[])property.Value));

                }
                else if (property.Name.Equals("PHOTO", StringComparison.OrdinalIgnoreCase) && valueType == typeof(string))
                {
                    //already base64 encoded
                    builder.Append(";ENCODING=b:");
                    builder.Append(property.Value);
                }
                else if (valueType == typeof(vCardValueCollection))
                {

                    vCardValueCollection values = (vCardValueCollection)property.Value;

                    builder.Append(':');
                    for (int index = 0; index < values.Count; index++)
                    {

                        builder.Append(EncodeEscaped(values[index]));
                        if (index < values.Count - 1)
                        {
                            builder.Append(values.Separator);
                        }
                    }

                }
                else
                {

                    // The object will be converted to a string (if it is
                    // not a string already) and encoded if necessary.
                    // The first step is to get the string value.

                    string stringValue = null;

                    if (valueType == typeof(char[]))
                    {
                        stringValue = new string(((char[])property.Value));
                    }
                    else
                    {
                        stringValue = property.Value.ToString();
                    }

                    builder.Append(':');

                    switch (property.Subproperties.GetValue("ENCODING"))
                    {

                        case "QUOTED-PRINTABLE":
                            builder.Append(EncodeQuotedPrintable(stringValue));
                            break;

                        default:
                            builder.Append(EncodeEscaped(stringValue));
                            break;

                    }

                }

            }

            return builder.ToString();

        }


        /// <summary>
        ///     Writes a vCard to an output text writer.
        /// </summary>
        public override void Write(vCard card, TextWriter output)
        {

            if (card == null)
                throw new ArgumentNullException("card");

            if (output == null)
                throw new ArgumentNullException("output");

            // Get the properties of the vCard.

            vCardPropertyCollection properties =
                BuildProperties(card);

            Write(properties, output);

        }

        /// <summary>
        ///     Writes a collection of vCard properties to an output text writer.
        /// </summary>
        public void Write(vCardPropertyCollection properties, TextWriter output)
        {

            if (properties == null)
                throw new ArgumentNullException("properties");

            if (output == null)
                throw new ArgumentNullException("output");

            foreach (vCardProperty property in properties)
            {
                output.WriteLine(EncodeProperty(property));
            }

        }

    }

}