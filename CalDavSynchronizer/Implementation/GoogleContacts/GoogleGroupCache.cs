using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleGroupCache : IGoogleGroupCache
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

    public string DefaultGroupIdOrNull
    {
      get { return _defaultGroupIdOrNull; }
    }

    public void Fill ()
    {
      var groups = new List<Group>();

      for (
       Feed<Group> feed = _apiOperationExecutor.Execute (f => f.GetGroups());
       feed != null;
       feed = _apiOperationExecutor.Execute (f => f.Get (feed, FeedRequestType.Next)))
      {
        groups.AddRange(feed.Entries);
      }

      SetGroups (groups);
    }

    void SetGroups (IEnumerable<Group> existingGroups)
    {
      foreach (var group in existingGroups)
      {
        Group existingGroup;
        Group winningGroup;
        if (_groupsByName.TryGetValue (group.Title, out existingGroup))
        {
          if (!string.IsNullOrEmpty (existingGroup.SystemGroup))
          {
            winningGroup = existingGroup;
          }
          else if (!string.IsNullOrEmpty (group.SystemGroup))
          {
            winningGroup = group;
          }
          else
          {
            if (string.CompareOrdinal (group.Id, existingGroup.Id) > 0)
              winningGroup = group;
            else
              winningGroup = existingGroup;
          }
        }
        else
        {
          winningGroup = group;
        }

        _groupsByName[winningGroup.Title] = winningGroup;

        if (IsDefaultGroup (winningGroup))
          _defaultGroupIdOrNull = winningGroup.Id;
      }
    }
    
    public Group GetOrCreateGroup (string groupName)
    {
      Group group;
      if (!_groupsByName.TryGetValue (groupName, out group))
      {
        group = CreateGroup (groupName);
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

    Group CreateGroup (string name)
    {
      var groupRequest = new Group();
      groupRequest.Title = name;

      return _apiOperationExecutor.Execute (f => f.Insert (new Uri ("https://www.google.com/m8/feeds/groups/default/full"), groupRequest));
    }


    public void AddDefaultGroupToContact (Contact contact)
    {
      if (_defaultGroupIdOrNull == null)
        return;

      contact.GroupMembership.Add (new GroupMembership() { HRef = _defaultGroupIdOrNull });
    }
  }
}