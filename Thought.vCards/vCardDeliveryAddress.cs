
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Generic;

namespace Thought.vCards
{

    /// <summary>
    ///     A postal address.
    /// </summary>
    /// <seealso cref="vCardDeliveryAddressCollection"/>
    [Serializable]
    public class vCardDeliveryAddress
    {

        private List<vCardDeliveryAddressTypes> addressType;
        private string city;
        private string country;
        private string postalCode;
        private string region;
        private string street;


        /// <summary>
        ///     Creates a new delivery address object.
        /// </summary>
        public vCardDeliveryAddress()
        {
            this.city = string.Empty;
            this.country = string.Empty;
            this.postalCode = string.Empty;
            this.region = string.Empty;
            this.street = string.Empty;
			this.addressType = new List<vCardDeliveryAddressTypes>();
        }


        /// <summary>
        ///     The type of postal address.
        /// </summary>
		public List<vCardDeliveryAddressTypes> AddressType
        {
            get
            {
                return this.addressType;
            }
            set
            {
                this.addressType = value;
            }
        }


        /// <summary>
        ///     The city or locality of the address.
        /// </summary>
        public string City
        {
            get
            {
                return this.city ?? string.Empty;
            }
            set
            {
                this.city = value;
            }
        }


        /// <summary>
        ///     The country name of the address.
        /// </summary>
        public string Country
        {
            get
            {
                return this.country ?? string.Empty;
            }
            set
            {
                this.country = value;
            }
        }


        /// <summary>
        ///     Indicates a domestic delivery address.
        /// </summary>
        public bool IsDomestic
        {
            get
            {
                return (addressType.Contains(vCardDeliveryAddressTypes.Domestic));
            }
        }


        /// <summary>
        ///     Indicates a home address.
        /// </summary>
        public bool IsHome
        {
            get
            {
				return (addressType.Contains(vCardDeliveryAddressTypes.Home));
            }
        }


        /// <summary>
        ///     Indicates an international address.
        /// </summary>
        public bool IsInternational
        {
            get
            {
				return (addressType.Contains(vCardDeliveryAddressTypes.International));
            }
        }


        /// <summary>
        ///     Indicates a parcel delivery address.
        /// </summary>
        public bool IsParcel
        {
            get
            {
				return (addressType.Contains(vCardDeliveryAddressTypes.Parcel));
            }
        }


        /// <summary>
        ///     Indicates a postal address.
        /// </summary>
        public bool IsPostal
        {
            get
            {
				return (addressType.Contains(vCardDeliveryAddressTypes.Postal));
            }
        }


        /// <summary>
        ///     Indicates a work address.
        /// </summary>
        public bool IsWork
        {
            get
            {
				return (addressType.Contains(vCardDeliveryAddressTypes.Work));
            }
        }

        /// <summary>
        ///     Indicates a preferred address
        /// </summary>
        public bool IsPreferred
        {
            get
            {
				return (addressType.Contains(vCardDeliveryAddressTypes.Preferred));
            }
        }


        /// <summary>
        ///     The postal code (e.g. ZIP code) of the address.
        /// </summary>
        public string PostalCode
        {
            get
            {
                return this.postalCode ?? string.Empty;
            }
            set
            {
                this.postalCode = value;
            }
        }


        /// <summary>
        ///     The region (state or province) of the address.
        /// </summary>
        public string Region
        {
            get
            {
                return this.region ?? string.Empty;
            }
            set
            {
                this.region = value;
            }
        }


        /// <summary>
        ///     The street of the delivery address.
        /// </summary>
        public string Street
        {
            get
            {
                return this.street ?? string.Empty;
            }
            set
            {
                this.street = value;
            }
        }

    }

}