
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
    ///     A collection of <see cref="vCardSource"/> objects.
    /// </summary>
    /// <seealso cref="vCardSource"/>
    public class vCardSourceCollection : Collection<vCardSource>
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="vCardSourceCollection"/>.
        /// </summary>
        public vCardSourceCollection()
            : base()
        {
        }

    }

}