
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Generic;
using System.Text;

namespace Thought.vCards
{

    /// <summary>
    ///     Extended options for the <see cref="vCardStandardWriter"/> class.
    /// </summary>
    [Flags]
    public enum vCardStandardWriterOptions
    {

        /// <summary>
        ///     No options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Indicates whether or not commas should be escaped in values.
        /// </summary>
        /// <remarks>
        ///     The vCard specification requires that commas be escaped
        ///     in values (e.g. a "," is translated to "\,").  However, Microsoft
        ///     Outlook(tm) does not properly decode these escaped commas.  This
        ///     option instruct the writer to ignored (not translate) embedded
        ///     commas for better compatibility with Outlook.
        /// </remarks>
        IgnoreCommas = 1

    }

}