using System.Collections.Generic;
using Google.Apis.PeopleService.v1.Data;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
    public interface IGoogleGroupCache
    {
        IEnumerable<ContactGroup> Groups { get; }
        ContactGroup GetOrCreateGroup(string groupName);
        bool IsDefaultGroupId(string id);
        bool IsDefaultGroup(ContactGroup group);
        void AddDefaultGroupToContact(Person contact);
    }
}