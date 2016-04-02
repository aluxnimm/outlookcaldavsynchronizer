using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Contacts;
using Google.GData.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleGroupCache
  {
    readonly Dictionary<string, Group> _groupsByName = new Dictionary<string, Group>();
    private readonly IGoogleApiOperationExecutor _apiOperationExecutor;

    private string _defaultGroupIdOrNull;

    public GoogleGroupCache (IGoogleApiOperationExecutor apiOperationExecutor)
    {
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));

      _apiOperationExecutor = apiOperationExecutor;
    }

    public IEnumerable<Group> Groups => _groupsByName.Values;

    public async Task Fill ()
    {
      SetGroups (await Task.Run(() => _apiOperationExecutor.Execute(f => f.GetGroups().Entries)));
    }

    void SetGroups (IEnumerable<Group> existingGroups)
    {
      foreach (var group in existingGroups)
      {
        _groupsByName.Add (group.Title, group);
        if (IsDefaultGroup (group))
          _defaultGroupIdOrNull = group.Id;
      }
    }

    public async Task<Group> GetOrCreateGroup (string groupName)
    {
      Group group;
      if (!_groupsByName.TryGetValue (groupName, out group))
      {
        group = await CreateGroup (groupName);
        _groupsByName.Add (groupName, group);
      }

      return group;
    }

    public bool IsDefaultGroupId (string id)
    {
      if (_defaultGroupIdOrNull == null)
        return false;

      return StringComparer.InvariantCultureIgnoreCase.Compare (_defaultGroupIdOrNull, id) == 0;
    }

    public bool IsDefaultGroup (Group group)
    {
      return StringComparer.InvariantCultureIgnoreCase.Compare (group.SystemGroup, "contacts") == 0;
    }

    Task<Group> CreateGroup (string name)
    {
      return Task.Run (() =>
      {
        var groupRequest = new Group();
        groupRequest.Title = name;

        return _apiOperationExecutor.Execute(f => f.Insert (new Uri ("https://www.google.com/m8/feeds/groups/default/full"), groupRequest));
      });
    }


    public void AddDefaultGroupToContact (Contact contact)
    {
      if (_defaultGroupIdOrNull == null)
        return;

      contact.GroupMembership.Add (new GroupMembership() { HRef = _defaultGroupIdOrNull });
    }
  }
}