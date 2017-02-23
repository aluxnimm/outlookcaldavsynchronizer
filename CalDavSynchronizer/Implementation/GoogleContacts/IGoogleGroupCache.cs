using System.Collections.Generic;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public interface IGoogleGroupCache
  {
    IEnumerable<Group> Groups { get; }
    Group GetOrCreateGroup (string groupName);
    bool IsDefaultGroupId (string id);
    bool IsDefaultGroup (Group group);
    void AddDefaultGroupToContact (Contact contact);
  }
}