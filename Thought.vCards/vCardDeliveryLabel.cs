
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
    ///     A formatted delivery label.
    /// </summary>
    /// <seealso cref="vCardDeliveryAddress"/>
    /// <seealso cref="vCardDeliveryLabelCollection"/>
    public class vCardDeliveryLabel
    {

		private List<vCardDeliveryAddressTypes> addressType;
        private string text;


        /// <summary>
        ///     Initializes a new <see cref="vCardDeliveryLabel"/>.
        /// </summary>
        public vCardDeliveryLabel()
        {
			this.addressType = new List<vCardDeliveryAddressTypes>();
        }


        /// <summary>
        ///     Initializes a new <see cref="vCardDeliveryLabel"/> to
        ///     the specified text.
        /// </summary>
        /// <param name="text">
        ///     The formatted text of a delivery label.  The label 
        ///     may contain carriage returns, line feeds, and other
        ///     control characters.
        /// </param>
        public vCardDeliveryLabel(string text)
        {
            this.text = text == null ? string.Empty : text;
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
        ///     The formatted delivery text.
        /// </summary>
        public string Text
        {
            get
            {
                return this.text ?? string.Empty;
            }
            set
            {
                this.text = value;
            }
        }

    }

}