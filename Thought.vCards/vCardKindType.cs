/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{
    /// <summary>
    ///     The kind of the contact.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Kind is not directly supported in the vCard 3.0 specification but expressed as X-ADDRESSBOOK-SERVER-KIND.
    ///     </para>
    /// </remarks>
    /// <seealso cref="vCard.Kind"/>
    public enum vCardKindType
    {
        /// <summary>
        ///     individual vCard.
        /// </summary>
        Individual = 0,


        /// <summary>
        ///     group vCard.
        /// </summary>
        Group = 1,


        /// <summary>
        ///     organization vCard.
        /// </summary>
        Org = 2,


        /// <summary>
        ///     location vCard.
        /// </summary>
        Location = 3
    }
}