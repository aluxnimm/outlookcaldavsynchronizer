
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Thought.vCards
{

    /// <summary>
    ///     Base class for vCard generators.
    /// </summary>
    /// <seealso cref="vCardReader"/>
    /// <seealso cref="vCardStandardWriter"/>
    public abstract class vCardWriter
    {


        /// <summary>
        ///     Holds output warnings.
        /// </summary>
        private StringCollection warnings = new StringCollection();


        /// <summary>
        ///     A collection of warning messages that were generated
        ///     during the output of a vCard.
        /// </summary>
        public StringCollection Warnings
        {
            get
            {
                return this.warnings;
            }
        }


        /// <summary>
        ///     Writes a vCard to an I/O stream using the format
        ///     implemented by the class.
        /// </summary>
        /// <param name="card">
        ///     The vCard to write the I/O string.
        /// </param>
        /// <param name="output">
        ///     The text writer to use for output.
        /// </param>
        /// <remarks>
        ///     The implementor should not close or flush the stream.
        ///     The caller owns the stream and may not wish for the
        ///     stream to be closed (e.g. the caller may call the
        ///     function again with a different vCard).
        /// </remarks>
        public abstract void Write(vCard card, TextWriter output);


        /// <summary>
        ///     Writes the vCard to the specified filename.
        /// </summary>
        public virtual void Write(vCard card, string filename)
        {

            if (card == null)
                throw new ArgumentNullException("card");

            using (StreamWriter output = new StreamWriter(filename))
            {
                Write(card, output);
            }

        }

    }

}