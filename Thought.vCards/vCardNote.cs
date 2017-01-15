
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

namespace Thought.vCards
{

    /// <summary>
    ///     A note or comment in a vCard.
    /// </summary>
    public class vCardNote
    {

        private string language;
        private string text;


        /// <summary>
        ///     Initializes a new vCard note.
        /// </summary>
        public vCardNote()
        {
        }


        /// <summary>
        ///     Initializes a new vCard note with the specified text.
        /// </summary>
        /// <param name="text">
        ///     The text of the note or comment.
        /// </param>
        public vCardNote(string text)
        {
            this.text = text;
        }


        /// <summary>
        ///     The language of the note.
        /// </summary>
        public string Language
        {
            get
            {
                return this.language ?? string.Empty;
            }
            set
            {
                this.language = value;
            }
        }

        /// <summary>
        ///     The text of the note.
        /// </summary>
        public string Text
        {
            get
            {
                return this.text ?? string.Empty;
            }
            set
            {
                this.text = value;
            }
        }


        /// <summary>
        ///     Returns the text of the note.
        /// </summary>
        public override string ToString()
        {
            return this.text ?? string.Empty;
        }

    }

}