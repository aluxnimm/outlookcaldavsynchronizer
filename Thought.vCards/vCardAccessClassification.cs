
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     The access classification of a vCard.
    /// </summary>
    /// <remarks>
    ///     The access classification defines the intent of the vCard owner.
    /// </remarks>
    public enum vCardAccessClassification
    {

        /// <summary>
        ///     The vCard classification is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     The vCard is classified as public.
        /// </summary>
        Public,

        /// <summary>
        ///     The vCard is classified as private.
        /// </summary>
        Private,

        /// <summary>
        ///     The vCard is classified as confidential.
        /// </summary>
        Confidential

    }

}