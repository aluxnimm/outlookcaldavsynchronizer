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
    ///     A collection of vcard group members.
    /// </summary>
    public class vCardMemberCollection : Collection<vCardMember>
    {
        /// <summary>
        ///    Initializes a new instance of the <see cref="vCardMemberCollection"/>.
        /// </summary>
        public vCardMemberCollection()
            : base()
        {
        }

        /// <summary>
        ///     Adds a new member to the collection.
        /// </summary>
        /// <param name="emailAddress">
        ///     The email address of the member.
        /// </param>
        /// <param name="displayName">
        ///     The displayName the member.
        /// </param>
        /// <returns>
        ///     The <see cref="vCardMember"/> object representing the note.
        /// </returns>
        public vCardMember Add(string emailAddress, string displayName)
        {
            vCardMember member = new vCardMember(emailAddress, displayName);
            Add(member);
            return member;
        }
    }
}