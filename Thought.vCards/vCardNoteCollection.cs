
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
    ///     A collection of <see cref="vCardNote"/> objects.
    /// </summary>
    public class vCardNoteCollection : Collection<vCardNote>
    {

        /// <summary>
        ///    Initializes a new instance of the <see cref="vCardNoteCollection"/>.
        /// </summary>
        public vCardNoteCollection()
            : base()
        {
        }

        /// <summary>
        ///     Adds a new note to the collection.
        /// </summary>
        /// <param name="text">
        ///     The text of the note.
        /// </param>
        /// <returns>
        ///     The <see cref="vCardNote"/> object representing the note.
        /// </returns>
        public vCardNote Add(string text)
        {

            vCardNote note = new vCardNote(text);
            Add(note);
            return note;

        }

    }

}