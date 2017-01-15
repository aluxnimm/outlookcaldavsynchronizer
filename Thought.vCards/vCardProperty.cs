
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Text;

namespace Thought.vCards
{

    /// <summary>
    ///     A property of a <see cref="vCard"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A vCard property specifies a single piece of information,
    ///         such as an email address or telephone number.  A property
    ///         can also specify meta-data like a revision number.  A full
    ///         vCards is basically a collection of properties structured
    ///         into a computer-friendly text format.
    ///     </para>
    ///     <para>
    ///         A property has a name, a value, and optionally one or
    ///         more subproperties.  A subproperty provides additional
    ///         information about the property (such as the encoding 
    ///         used to store the value).  The format of a value 
    ///         depends on the property and in some cases may be broken
    ///         into multiple values.
    ///     </para>
    /// </remarks>
    /// <seealso cref="vCardPropertyCollection"/>
    public class vCardProperty
    {

        private string group;
        private string language;
        private string name;
        private vCardSubpropertyCollection subproperties;
        private object value;


        /// <summary>
        ///     Creates a blank <see cref="vCardProperty"/> object.
        /// </summary>
        public vCardProperty()
        {
            this.subproperties = new vCardSubpropertyCollection();
        }


        /// <summary>
        ///     Creates a <see cref="vCardProperty"/> object
        ///     with the specified name and a null value.
        /// </summary>
        /// <param name="name">
        ///     The name of the property.
        /// </param>
        public vCardProperty(string name)
        {
            this.name = name;
            this.subproperties = new vCardSubpropertyCollection();
        }


        /// <summary>
        ///     Creates a <see cref="vCardProperty"/> object with the
        ///     specified name and value.
        /// </summary>
        /// <remarks>
        ///     The vCard specification supports multiple values in
        ///     certain fields, such as the N field.  The value specified
        ///     in this constructor is loaded as the first value.
        /// </remarks>
        public vCardProperty(string name, string value)
        {
            this.name = name;
            this.subproperties = new vCardSubpropertyCollection();
            this.value = value;
        }


        /// <summary>
        ///     Initializes a vCardProperty with the specified
        ///     name, value and group.
        /// </summary>
        /// <param name="name">
        ///     The name of the vCard property.
        /// </param>
        /// <param name="value">
        ///     The value of the vCard property.
        /// </param>
        /// <param name="group">
        ///     The group name of the vCard property.
        /// </param>
        public vCardProperty(string name, string value, string group)
        {
            this.group = group;
            this.name = name;
            this.subproperties = new vCardSubpropertyCollection();
            this.value = value;
        }


        /// <summary>
        ///     Creates a <see cref="vCardProperty"/> with the
        ///     specified name and a byte array as a value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value as a byte array.</param>
        public vCardProperty(string name, byte[] value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.name = name;
            this.subproperties = new vCardSubpropertyCollection();
            this.value = value;
        }


        /// <summary>
        ///     Creates a <see cref="vCardProperty"/> with
        ///     the specified name and date/time as a value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The date/time value.</param>
        public vCardProperty(string name, DateTime value)
        {

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.name = name;
            this.subproperties = new vCardSubpropertyCollection();
            this.value = value;
        }


        /// <summary>
        ///     Initializes the vCard property with the specified
        ///     name and values.
        /// </summary>
        public vCardProperty(string name, vCardValueCollection values)
            : this()
        {

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (values == null)
                throw new ArgumentNullException("values");

            this.subproperties = new vCardSubpropertyCollection();
            this.name = name;
            this.value = values;
        }


        /// <summary>
        ///     The group name of the property.
        /// </summary>
        public string Group
        {
            get
            {
                return this.group ?? string.Empty;
            }
            set
            {
                this.group = value;
            }
        }


        /// <summary>
        ///     The language code of the property.
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
        ///     The name of the property (e.g. TEL).
        /// </summary>
        public string Name
        {
            get
            {
                return this.name ?? string.Empty;
            }
            set
            {
                this.name = value;
            }
        }


        /// <summary>
        ///     Subproperties of the vCard property, not including
        ///     the name, encoding, and character set.
        /// </summary>
        public vCardSubpropertyCollection Subproperties
        {
            get
            {
                return this.subproperties;
            }
        }


        /// <summary>
        ///     Returns the value of the property as a string.
        /// </summary>
        public override string ToString()
        {
            if (value == null)
            {
                return string.Empty;
            }
            else
            {
                return value.ToString();
            }

        }


        /// <summary>
        ///     The value of the property.
        /// </summary>
        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }


    }

}