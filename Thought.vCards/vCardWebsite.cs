
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     A web site defined in a vCard.
    /// </summary>
    /// <seealso cref="vCardWebsiteCollection"/>
    /// <seealso cref="vCardWebsiteTypes"/>
    public class vCardWebsite
    {

        private string url;
        private vCardWebsiteTypes websiteType;


        /// <summary>
        ///     Creates a vCardWebSite object.
        /// </summary>
        public vCardWebsite()
        {
            this.url = string.Empty;
        }


        /// <summary>
        ///     Creates a new vCardWebSite object with the specified URL.
        /// </summary>
        /// <param name="url">
        ///     The URL of the web site.
        /// </param>
        public vCardWebsite(string url)
        {
            this.url = url == null ? string.Empty : url;
        }


        /// <summary>
        ///     Creates a new vCardWebSite with the
        ///     specified URL and classification.
        /// </summary>
        /// <param name="url">
        ///     The URL of the web site.
        /// </param>
        /// <param name="websiteType">
        ///     The classification of the web site.
        /// </param>
        public vCardWebsite(string url, vCardWebsiteTypes websiteType)
        {
            this.url = url == null ? string.Empty : url;
            this.websiteType = websiteType;
        }


        /// <summary>
        ///     Indicates a personal home page.
        /// </summary>
        public bool IsPersonalSite
        {
            get
            {
                return (this.websiteType & vCardWebsiteTypes.Personal) ==
                    vCardWebsiteTypes.Personal;
            }
            set
            {

                if (value)
                {
                    this.websiteType |= vCardWebsiteTypes.Personal;
                }
                else
                {
                    this.websiteType &= ~vCardWebsiteTypes.Personal;
                }

            }
        }


        /// <summary>
        ///     Indicates a work-related web site.
        /// </summary>
        public bool IsWorkSite
        {
            get
            {
                return (this.websiteType & vCardWebsiteTypes.Work) ==
                    vCardWebsiteTypes.Work;
            }
            set
            {

                if (value)
                {
                    this.websiteType |= vCardWebsiteTypes.Work;
                }
                else
                {
                    this.websiteType &= ~vCardWebsiteTypes.Work;
                }

            }

        }


        /// <summary>
        ///     The URL of the web site.
        /// </summary>
        /// <remarks>
        ///     The format of the URL is not validated.
        /// </remarks>
        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                if (value == null)
                {
                    this.url = string.Empty;
                }
                else
                {
                    this.url = value;
                }
            }
        }


        /// <summary>
        ///     The type of web site (e.g. home page, work, etc).
        /// </summary>
        public vCardWebsiteTypes WebsiteType
        {
            get
            {
                return this.websiteType;
            }
            set
            {
                this.websiteType = value;
            }
        }


        /// <summary>
        ///     Returns the string representation (URL) of the web site.
        /// </summary>
        /// <returns>
        ///     The URL of the web site.
        /// </returns>
        public override string ToString()
        {
            return this.url;
        }
    }

}