
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Specialized;
using System.Text;

namespace Thought.vCards
{

    /// <summary>
    ///     A collection of string values.
    /// </summary>
    public class vCardValueCollection : StringCollection
    {

        private char separator;

        /// <summary>
        ///     Initializes an empty <see cref="vCardValueCollection"/>.
        /// </summary>
        public vCardValueCollection()
            : base()
        {
            this.separator = ',';
        }


        /// <summary>
        ///     Initializes the value collection with the specified separator.
        /// </summary>
        /// <param name="separator">
        ///     The suggested character to use as a separator when
        ///     writing the collection as a string.
        /// </param>
        public vCardValueCollection(char separator)
        {
            this.separator = separator;
        }


        /// <summary>
        ///     Adds the contents of a StringCollection to the collection.
        /// </summary>
        /// <param name="values">
        ///     An initialized StringCollection containing zero or more values.
        /// </param>
        public void Add(StringCollection values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            foreach (string value in values)
                Add(value);

        }


        /// <summary>
        ///     The suggested separator when writing values to a string.
        /// </summary>
        public char Separator
        {
            get
            {
                return this.separator;
            }
            set
            {
                this.separator = value;
            }
        }

    }

}