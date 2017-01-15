
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     Identifies the type of email address in a vCard.
    /// </summary>
    /// <seealso cref="vCardEmailAddress"/>
    public enum vCardEmailAddressType
    {

        /// <summary>
        ///     An Internet (SMTP) mail (default) address.
        /// </summary>
        Internet = 0,


        /// <summary>
        ///     An America On-Line email address.
        /// </summary>
        AOL,


        /// <summary>
        ///     An AppleLink email address.
        /// </summary>
        AppleLink,


        /// <summary>
        ///     An AT&amp;T Mail email address
        /// </summary>
        AttMail,


        /// <summary>
        ///     A CompuServe Information Service (CIS) email address.
        /// </summary>
        CompuServe,


        /// <summary>
        ///     An eWorld email address.
        /// </summary>
        /// <remarks>
        ///     eWorld was an online service by Apple Computer in the mid 1990s.
        ///     It was officially shut down on March 31, 1996.
        /// </remarks>
        eWorld,


        /// <summary>
        ///     An IBM Mail email address.
        /// </summary>
        IBMMail,


        /// <summary>
        ///     An MCI Mail email address.
        /// </summary>
        MCIMail,


        /// <summary>
        ///     A PowerShare email address.
        /// </summary>
        PowerShare,


        /// <summary>
        ///     A Prodigy Information Service email address.
        /// </summary>
        Prodigy,


        /// <summary>
        ///     A telex email address.
        /// </summary>
        Telex,


        /// <summary>
        ///     An X.400 service email address.
        /// </summary>
        X400

    }

    /// <summary>
    /// Identifies the HOME,WORK,ETC typing of an element
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// default - unknown
        /// </summary>
        UNSPECIFIED =0,
        /// <summary>
        /// work
        /// </summary>
        WORK = 1,
        /// <summary>
        /// home
        /// </summary>
        HOME =2,

    }

}