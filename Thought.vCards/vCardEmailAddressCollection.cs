
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.ObjectModel;

namespace Thought.vCards
{

    /// <summary>
    ///     A collection of <see cref="vCardEmailAddress"/> objects.
    /// </summary>
    /// <seealso cref="vCardEmailAddress"/>
    /// <seealso cref="vCardEmailAddressType"/>
    public class vCardEmailAddressCollection : Collection<vCardEmailAddress>
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="vCardEmailAddressCollection" />.
        /// </summary>
        public vCardEmailAddressCollection()
            : base()
        {
        }

        /// <summary>
        ///     Locates the first email address of the specified type while 
        ///     giving preference to email addresses marked as preferred.
        /// </summary>
        /// <param name="emailType">
        ///     The type of email address to locate.  This can be any 
        ///     combination of values from <see cref="vCardEmailAddressType"/>.
        /// </param>
        /// <returns>
        ///     The function returns the first preferred email address that matches
        ///     the specified type.  If the collection does not contain a preferred
        ///     email address, then it will return the first non-preferred matching
        ///     email address.  The function returns null if no matches were found.
        /// </returns>
        public vCardEmailAddress GetFirstChoice(vCardEmailAddressType emailType)
        {

            vCardEmailAddress firstNonPreferred = null;

            foreach (vCardEmailAddress email in this)
            {

                if ((email.EmailType & emailType) == emailType)
                {

                    if (firstNonPreferred == null)
                        firstNonPreferred = email;

                    if (email.IsPreferred)
                        return email;
                }

            }

            return firstNonPreferred;

        }

    }

}