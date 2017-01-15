
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     Base class for <see cref="vCard"/>-specific exceptions.
    /// </summary>
    [Serializable]
    public class vCardException : Exception
    {


        /// <summary>
        ///     Initializes a new instance of the vCardException class.
        /// </summary>
        public vCardException()
            : base()
        {
        }


        /// <summary>
        ///     Initializes a new instance of the vCardException
        ///     class with the specified error message.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        public vCardException(string message)
            : base(message)
        {
        }


        /// <summary>
        ///     Initializes a new instance of the vCardException
        ///     class with a specified error message and a reference
        ///     to the inner exception that is the cause of the
        ///     exception.
        /// </summary>
        /// <param name="message">
        ///     The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception,
        ///     or a null reference (Nothing in Visual Basic) if no
        ///     inner exception is specified.
        /// </param>
        public vCardException(string message, Exception innerException)
            : base(message, innerException)
        {
        }



    }

}