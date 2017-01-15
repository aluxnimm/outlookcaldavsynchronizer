
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     An email address in a <see cref="vCard"/>.
    /// </summary>
    /// <remarks>
    ///     Most vCard email addresses are Internet email addresses.  However,
    ///     the vCard specification allows other email address formats,
    ///     such as CompuServe and X400.  Unless otherwise specified, an
    ///     address is assumed to be an Internet address.
    /// </remarks>
    /// <seealso cref="vCardEmailAddressCollection"/>
    /// <seealso cref="vCardEmailAddressType"/>
    public class vCardEmailAddress
    {

        private string address;
        private vCardEmailAddressType emailType;
        private ItemType itemType;
        private bool isPreferred;
        
   
        /// <summary>
        ///     Creates a new <see cref="vCardEmailAddress"/>.
        /// </summary>
        public vCardEmailAddress()
        {
            this.address = string.Empty;
            this.emailType = vCardEmailAddressType.Internet;
            this.itemType = ItemType.UNSPECIFIED;
        }


        /// <summary>
        ///     Creates a new Internet <see cref="vCardEmailAddress"/>.
        /// </summary>
        /// <param name="address">
        ///     The Internet email address.
        /// </param>
        /// <param name="emailType">type of address, usually Internet. Internet is the default.</param>
        /// <param name="itemType">HOME,WORK, unspecified</param>
        public vCardEmailAddress(string address, vCardEmailAddressType emailType = vCardEmailAddressType.Internet, ItemType itemType = ItemType.UNSPECIFIED)
        {
            this.address = address == null ? string.Empty : address;
            this.emailType = emailType;
            this.itemType = itemType;
        }


 


        /// <summary>
        ///     The email address.
        /// </summary>
        /// <remarks>
        ///     The format of the email address is not validated by the class.
        /// </remarks>
        public string Address
        {
            get
            {
                return this.address ?? string.Empty;
            }
            set
            {
                this.address = value;
            }
        }


        /// <summary>
        ///     The email address type.
        /// </summary>
        public vCardEmailAddressType EmailType
        {
            get
            {
                return this.emailType;
            }
            set
            {
                this.emailType = value;
            }
        }


        /// <summary>
        ///     Indicates a preferred (top priority) email address.
        /// </summary>
        public bool IsPreferred
        {
            get
            {
                return this.isPreferred;
            }
            set
            {
                this.isPreferred = value;
            }
        }

        /// <summary>
        /// ItemType for this element (HOME,WORK,etc)
        /// </summary>
        public ItemType ItemType {

            get { return this.itemType; }
            set { this.itemType = value; }
        
        }


        /// <summary>
        ///     Builds a string that represents the email address.
        /// </summary>
        public override string ToString()
        {
            return this.address;
        }
    }

}