
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
    ///     A generic collection <see cref="vCardPhone"/> objects.
    /// </summary>
    /// <seealso cref="vCardPhone"/>
    /// <seealso cref="vCardPhoneTypes"/>
    public class vCardPhoneCollection : Collection<vCardPhone>
    {


        /// <summary>
        ///     Looks for the first phone of the specified
        ///     type that is a preferred phone.
        /// </summary>
        /// <param name="phoneType">
        ///     The type of phone to seek.
        /// </param>
        /// <returns>
        ///     The first <see cref="vCardPhone "/> that matches
        ///     the specified type.  A preferred number is returned
        ///     before a non-preferred number.
        /// </returns>
        public vCardPhone GetFirstChoice(vCardPhoneTypes phoneType)
        {

            vCardPhone firstNonPreferred = null;

            foreach (vCardPhone phone in this)
            {

                if ((phone.PhoneType & phoneType) == phoneType)
                {

                    // This phone has the same phone type as
                    // specified by the caller.  Save a reference
                    // to the first such phone encountered.

                    if (firstNonPreferred == null)
                        firstNonPreferred = phone;

                    if (phone.IsPreferred)
                        return phone;
                }

            }

            // No phone had the specified phone type and was marked
            // as preferred.

            return firstNonPreferred;
        }

    }

}