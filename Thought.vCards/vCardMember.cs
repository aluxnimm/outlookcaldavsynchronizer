
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

namespace Thought.vCards
{

    /// <summary>
    ///     A member in a vCard.
    /// </summary>
    public class vCardMember
    {

        private string emailAddress;
        private string displayName;


        /// <summary>
        ///     Initializes a new vCard member.
        /// </summary>
        public vCardMember()
        {
        }


        /// <summary>
        ///     Initializes a new vCard member with the specified emailaddress and displayname.
        /// </summary>
        /// <param name="emailAddress">
        ///     The email of the member.
        /// </param>
        /// <param name="displayName">
        ///     The displayname of the member.
        /// </param>
        public vCardMember(string emailAddress, string displayName)
        {
            this.emailAddress = emailAddress;
            this.displayName = displayName;
        }


        /// <summary>
        ///     The email of the member.
        /// </summary>
        public string EmailAddress
        {
            get
            {
                return this.emailAddress ?? string.Empty;
            }
            set
            {
                this.emailAddress = value;
            }
        }

        /// <summary>
        ///     The displayname of the member.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this.displayName ?? string.Empty;
            }
            set
            {
                this.displayName = value;
            }
        }


        /// <summary>
        ///     Returns the email of the member.
        /// </summary>
        public override string ToString()
        {
            return this.emailAddress ?? string.Empty;
        }

    }

}