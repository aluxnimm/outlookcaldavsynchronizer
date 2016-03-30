using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleGroupCache
  {
    readonly Dictionary<string, Group> _groupsByName = new Dictionary<string, Group>();
    private readonly ContactsRequest _contactFacade;

    public GoogleGroupCache (ContactsRequest contactFacade)
    {
      if (contactFacade == null)
        throw new ArgumentNullException (nameof (contactFacade));

      _contactFacade = contactFacade;
    }

    public void SetGroups (IEnumerable<Group> existingGroups)
    {
      foreach (var group in existingGroups)
        _groupsByName.Add (group.Title, group);
    }

    public async Task<string> GetOrCreateGroupId (string groupName)
    {
      Group group;
      if (!_groupsByName.TryGetValue (groupName, out group))
      {
        group = await CreateGroup (groupName);
        _groupsByName.Add (groupName, group);
      }

      return group.Id;
    }

    Task<Group> CreateGroup (string name)
    {
      return Task.Run (() =>
      {
        var groupRequest = new Group ();
        groupRequest.Title = name;

        return _contactFacade.Insert (new Uri ("https://www.google.com/m8/feeds/groups/default/full"), groupRequest);
      });
    }
  }
}
