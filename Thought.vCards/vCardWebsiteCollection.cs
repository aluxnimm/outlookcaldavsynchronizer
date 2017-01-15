
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
    ///     A collection of <see cref="vCardWebsite"/> objects.
    /// </summary>
    /// <seealso cref="vCardWebsite"/>
    /// <seealso cref="vCardWebsiteTypes"/>
    public class vCardWebsiteCollection : Collection<vCardWebsite>
    {

        /// <summary>
        ///     Returns the first web site of the specified type.  If
        ///     the collection does not contain a website of the specified
        ///     type, but does contain a default (uncategorized) website,
        ///     then that website will be returned.
        /// </summary>
        /// <param name="siteType"></param>
        /// <returns></returns>
        public vCardWebsite GetFirstChoice(vCardWebsiteTypes siteType)
        {

            vCardWebsite alternate = null;

            foreach (vCardWebsite webSite in this)
            {

                if ((webSite.WebsiteType & siteType) == siteType)
                {
                    return webSite;
                }
                else
                {

                    if (
                        (alternate == null) &&
                        (webSite.WebsiteType == vCardWebsiteTypes.Default))
                    {
                        alternate = webSite;
                    }

                }
            }

            return alternate;

        }

    }

}