
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Specialized;
using System.IO;

namespace Thought.vCards
{

    /// <summary>
    ///     An abstract reader for vCard and vCard-like file formats.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Warnings"/> property is a string collection
    ///         containing a description of each warning encountered during
    ///         the read process.  An implementor of a card reader should
    ///         populate this collection as the vCard data is being parsed.
    ///     </para>
    /// </remarks>
    public abstract class vCardReader
    {

        /// <summary>
        ///     Stores the warnings issued by the implementor
        ///     of the vCard reader.  Currently warnings are
        ///     simple string messages; a future version will
        ///     store line numbers, severity levels, etc.
        /// </summary>
        /// <seealso cref="Warnings"/>
        private StringCollection warnings;


        /// <summary>
        ///     Initializes the base reader.
        /// </summary>
        protected vCardReader()
        {
            this.warnings = new StringCollection();
        }


        /// <summary>
        ///     Reads a vCard from the specified input stream.
        /// </summary>
        /// <param name="reader">
        ///     A text reader that points to the beginning of
        ///     a vCard in the format expected by the implementor.
        /// </param>
        /// <returns>
        ///     An initialized <see cref="vCard"/> object.
        /// </returns>
        public vCard Read(TextReader reader)
        {
            vCard card = new vCard();
            ReadInto(card, reader);
            return card;
        }


        /// <summary>
        ///     Reads vCard information from a text reader and
        ///     populates into an existing vCard object.
        /// </summary>
        /// <param name="card">
        ///     An initialized vCard object.
        /// </param>
        /// <param name="reader">
        ///     A text reader containing vCard data in the format
        ///     expected by the card reader class.
        /// </param>
        public abstract void ReadInto(vCard card, TextReader reader);


        /// <summary>
        ///     A collection of warning messages.
        /// </summary>
        /// <remarks>Reseved for future use.</remarks>
        public StringCollection Warnings
        {
            get
            {
                return this.warnings;
            }
        }

    }

}