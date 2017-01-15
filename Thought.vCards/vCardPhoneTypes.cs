
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     Identifies different phone types (e.g. Fax, BBS, etc).
    /// </summary>
    /// <seealso cref="vCardPhone"/>
    /// <seealso cref="vCardPhoneCollection"/>
    [Flags]
    public enum vCardPhoneTypes
    {

        /// <summary>
        ///     Indicates default properties.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Indicates a bulletin board system.
        /// </summary>
        BBS = 1,

        /// <summary>
        ///     Indicates a car phone.
        /// </summary>
        Car = 2,

        /// <summary>
        ///     Indicates a cell phone.
        /// </summary>
        Cellular = 4,

        /// <summary>
        ///     Indicates a celluar voice number.
        /// </summary>
        CellularVoice = Cellular + Voice,

        /// <summary>
        ///     Indicates a facsimile number.
        /// </summary>
        Fax = 8,

        /// <summary>
        ///     Indicates a home number
        /// </summary>
        Home = 16,

        /// <summary>
        ///     Indicates a home and voice number.
        /// </summary>
        HomeVoice = Home + Voice,

        /// <summary>
        ///     Indicates an ISDN number.
        /// </summary>
        ISDN = 32,

        /// <summary>
        ///     Indicates a messaging service on the number.
        /// </summary>
        MessagingService = 64,

        /// <summary>
        ///     Indicates a MODEM number.
        /// </summary>
        Modem = 128,

        /// <summary>
        ///     Indicates a pager number.
        /// </summary>
        Pager = 256,

        /// <summary>
        ///     Indicates a preferred number.
        /// </summary>
        Preferred = 512,

        /// <summary>
        ///     Indicates a video number.
        /// </summary>
        Video = 1024,

        /// <summary>
        ///     Indicates a voice number.
        /// </summary>
        Voice = 2048,

        /// <summary>
        ///     Indicates a work number.
        /// </summary>
        Work = 4096,

        /// <summary>
        ///     Indicates a work fax number.
        /// </summary>
        WorkFax = Work + Fax,

        /// <summary>
        ///     Indicates a work and voice number.
        /// </summary>
        WorkVoice = Work + Voice,

        /// <summary>
        /// to handle iPhone type in NAB for phone
        /// </summary>
        IPhone = 8192,

        /// <summary>
        /// to handle Main type for NAB
        /// </summary>
        Main = 16384

    }

}