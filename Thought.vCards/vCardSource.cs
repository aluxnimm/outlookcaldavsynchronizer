
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     A source of directory information for a vCard.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A source identifies a directory that contains or provided
    ///         information for the vCard.  A source consists of a URI
    ///         and a context.  The URI is generally a URL; the
    ///         context identifies the protocol and type of URI.  For
    ///         example, a vCard associated with an LDAP directory entry
    ///         will have an ldap:// URL and a context of "LDAP".
    ///     </para>
    /// </remarks>
    /// <seealso cref="vCardSourceCollection"/>
    public class vCardSource
    {

        private string context;
        private Uri uri;


        /// <summary>
        ///     Initializes a new instance of the vCardSource class.
        /// </summary>
        public vCardSource()
        {
            this.context = string.Empty;
        }


        /// <summary>
        ///     Initializes a new source with the specified URI.
        /// </summary>
        /// <param name="uri">
        ///     The URI of the directory entry.
        /// </param>
        public vCardSource(Uri uri)
        {
            this.uri = uri;
        }


        /// <summary>
        ///     Initializes a new source with the specified
        ///     context and URI.
        /// </summary>
        /// <param name="uri">
        ///     The URI of the source of the vCard data.
        /// </param>
        /// <param name="context">
        ///     The context of the source.
        /// </param>
        public vCardSource(Uri uri, string context)
        {
            this.context = context;
            this.uri = uri;
        }


        /// <summary>
        ///     The context of the source URI.
        /// </summary>
        /// <remarks>
        ///     The context identifies how the URI should be
        ///     interpreted.  Example is "LDAP", which indicates
        ///     the URI is an LDAP reference.
        /// </remarks>
        public string Context
        {
            get
            {
                return this.context ?? string.Empty;
            }
            set
            {
                this.context = value;
            }
        }


        /// <summary>
        ///     The URI of the source.
        /// </summary>
        public Uri Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }

    }

}