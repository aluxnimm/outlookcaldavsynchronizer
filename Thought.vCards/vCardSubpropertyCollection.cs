
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace Thought.vCards
{

    /// <summary>
    ///     A collection of <see cref="vCardSubproperty"/> objects.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is a general-purpose collection of
    ///         <see cref="vCardSubproperty"/> objects.
    ///     </para>
    ///     <para>
    ///         A property of a vCard contains a piece of
    ///         contact information, such as an email address
    ///         or web site.  A subproperty indicates options
    ///         or attributes of the property, such as the
    ///         type of email address or character set.
    ///     </para>
    /// </remarks>
    /// <seealso cref="vCardProperty"/>
    /// <seealso cref="vCardSubproperty"/>
    public class vCardSubpropertyCollection : Collection<vCardSubproperty>
    {

        /// <summary>
        ///     Adds a subproperty without a value.
        /// </summary>
        /// <param name="name">
        ///     The name of the subproperty.
        /// </param>
        public void Add(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            else
            {
                Add(new vCardSubproperty(name));
            }
        }


        /// <summary>
        ///     Adds a subproperty with the specified name and value.
        /// </summary>
        /// <param name="name">
        ///     The name of the new subproperty to add.
        /// </param>
        /// <param name="value">
        ///     The value of the new subproperty to add.  This can be null.
        /// </param>
        public void Add(string name, string value)
        {
            Add(new vCardSubproperty(name, value));
        }


        /// <summary>
        ///     Either adds or updates a subproperty with the specified name.
        /// </summary>
        /// <param name="name">
        ///     The name of the subproperty to add or update.
        /// </param>
        /// <param name="value">
        ///     The value of the subproperty to add or update.
        /// </param>
        public void AddOrUpdate(string name, string value)
        {

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            int index = IndexOf(name);

            if (index == -1)
            {
                Add(name, value);
            }
            else
            {
                this[index].Value = value;
            }

        }


        /// <summary>
        ///     Determines if the collection contains a subproperty
        ///     with the specified name.
        /// </summary>
        /// <param name="name">
        ///     The name of the subproperty.
        /// </param>
        /// <returns>
        ///     True if the collection contains a subproperty with the
        ///     specified name, or False otherwise.
        /// </returns>
        public bool Contains(string name)
        {

            foreach (vCardSubproperty sub in this)
            {

                if (string.Compare(name, sub.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;

        }


        /// <summary>
        ///     Builds a string array containing subproperty names.
        /// </summary>
        /// <returns>
        ///     A string array containing the unmodified name of
        ///     each subproperty in the collection.
        /// </returns>
        public string[] GetNames()
        {

            ArrayList names = new ArrayList(this.Count);

            foreach (vCardSubproperty sub in this)
            {
                names.Add(sub.Name);
            }

            return (string[])names.ToArray(typeof(string));

        }


        /// <summary>
        ///     Builds a string array containing all subproperty
        ///     names that match one of the names in an array.
        /// </summary>
        /// <param name="filteredNames">
        ///     A list of valid subproperty names.
        /// </param>
        /// <returns>
        ///     A string array containing the names of all subproperties
        ///     that match an entry in the filterNames list.
        /// </returns>
        public string[] GetNames(string[] filteredNames)
        {

            if (filteredNames == null)
                throw new ArgumentNullException("filteredNames");

            // The vCard specification is not case-sensitive.  
            // Therefore the subproperty names and the filter names
            // list must be compared in a case-insensitive matter.
            // Whitespace will also be ignored.  For better-
            // performing comparisons, a processed version of
            // the filtered list will be constructed.

            string[] processedNames =
                (string[])filteredNames.Clone();

            for (int index = 0; index < processedNames.Length; index++)
            {
                if (!string.IsNullOrEmpty(processedNames[index]))
                {
                    processedNames[index] =
                        processedNames[index].Trim().ToUpperInvariant();
                }
            }

            // Matching names will be stored in an array list,
            // and then converted to a string array for return.

            ArrayList matchingNames = new ArrayList();

            foreach (vCardSubproperty sub in this)
            {

                // Convert this subproperty name to upper case.
                // The names in the processed array are already
                // in upper case.

                string subName =
                    sub.Name == null ? null : sub.Name.ToUpperInvariant();

                // See if the processed subproperty name has any
                // matches in the processed array. 

                int matchIndex =
                    Array.IndexOf<string>(processedNames, subName);

                if (matchIndex != -1)
                    matchingNames.Add(processedNames[matchIndex]);

            }

            return (string[])matchingNames.ToArray(typeof(string));

        }


        /// <summary>
        ///     Get the value of the subproperty with
        ///     the specified name.
        /// </summary>
        /// <param name="name">
        ///     The name of the subproperty.
        /// </param>
        /// <returns>
        ///     The value of the subproperty or null if no
        ///     such subproperty exists in the collection.
        /// </returns>
        public string GetValue(string name)
        {

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // Get the collection index of the subproperty
            // object that has the specified name.

            int index = IndexOf(name);
            if (index == -1)
            {
                return null;
            }
            else
            {
                return this[index].Value;
            }

        }


        /// <summary>
        ///     Gets the value of the first subproperty with the
        ///     specified name, or the first value specified in
        ///     a list.
        /// </summary>
        /// <param name="name">
        ///     The expected name of the subproperty.
        /// </param>
        /// <param name="namelessValues">
        ///     A list of values that are sometimes listed as
        ///     subproperty names.  The first matching value is
        ///     returned if the name parameter does not match.
        /// </param>
        public string GetValue(
            string name,
            string[] namelessValues)
        {

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // See if the subproperty exists with the
            // specified name.  If so, return the value
            // immediately.

            int index = IndexOf(name);
            if (index != -1)
            {
                return this[index].Value;
            }

            // A subproperty with the specified name does
            // not exist.  However, this does not mean that
            // the subproperty does not exist.  Some subproperty
            // values can be written directly without a name.
            // An example is the ENCODING property.  Example:
            //
            // New Format: KEY;ENCODING=BASE64:....
            // Old Format: KEY;BASE64:...

            if ((namelessValues == null) || (namelessValues.Length == 0))
                return null;

            int nameIndex = IndexOfAny(namelessValues);
            if (nameIndex == -1)
            {
                return null;
            }
            else
            {
                return this[nameIndex].Name;
            }

        }


        /// <summary>
        ///     Searches for a subproperty with the specified name.
        /// </summary>
        /// <param name="name">
        ///     The name of the subproperty.
        /// </param>
        /// <returns>
        ///     The collection (zero-based) index of the first
        ///     subproperty that matches the specified name.  The
        ///     function returns -1 if no match is found.
        /// </returns>
        public int IndexOf(string name)
        {

            for (int index = 0; index < this.Count; index++)
            {
                if (string.Compare(name, this[index].Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return index;
                }
            }

            return -1;

        }


        /// <summary>
        ///     Finds the first subproperty that has any of the
        ///     specified names.
        /// </summary>
        /// <param name="names">
        ///     An array of names to search.
        /// </param>
        /// <returns>
        ///     The collection index of the first subproperty with
        ///     the specified name, or -1 if no subproperty was found.
        /// </returns>
        public int IndexOfAny(string[] names)
        {

            if (names == null)
                throw new ArgumentNullException("names");

            for (int index = 0; index < this.Count; index++)
            {

                foreach (string name in names)
                {

                    if (string.Compare(this[index].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return index;
                    }
                }

            }

            return -1;

        }


    }

}