
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     The type of a delivery address.
    /// </summary>
    public enum vCardDeliveryAddressTypes
    {

        /// <summary>
        ///     Default address settings.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     A domestic delivery address.
        /// </summary>
        Domestic,

        /// <summary>
        ///     An international delivery address.
        /// </summary>
        International,

        /// <summary>
        ///     A postal delivery address.
        /// </summary>
        Postal,

        /// <summary>
        ///     A parcel delivery address.
        /// </summary>
        Parcel,

        /// <summary>
        ///     A home delivery address.
        /// </summary>
        Home,

        /// <summary>
        ///     A work delivery address.
        /// </summary>
        Work,
        
        /// <summary>
        /// you can mark an address as Preferred type="pref" 
        /// </summary>
        Preferred
    }

}