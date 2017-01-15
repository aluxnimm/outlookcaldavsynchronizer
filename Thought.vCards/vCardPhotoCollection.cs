
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
    ///     A collection of <see cref="vCardPhoto"/> objects.
    /// </summary>
    /// <seealso cref="vCardPhoto"/>
    public class vCardPhotoCollection : Collection<vCardPhoto>
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="vCardPhotoCollection"/>.
        /// </summary>
        public vCardPhotoCollection()
            : base()
        {
        }

    }

}
