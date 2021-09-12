using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.PeopleService.v1.Data;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
    public class GoogleGroupCache : IGoogleGroupCache
    {
        readonly Dictionary<string, ContactGroup> _groupsByName = new Dictionary<string, ContactGroup>();
        private readonly IGoogleApiOperationExecutor _apiOperationExecutor;

        private string _defaultGroupResourceNameOrNull;

        public GoogleGroupCache(IGoogleApiOperationExecutor apiOperationExecutor)
        {
            if (apiOperationExecutor == null)
                throw new ArgumentNullException(nameof(apiOperationExecutor));

            _apiOperationExecutor = apiOperationExecutor;
        }

        public IEnumerable<ContactGroup> Groups => _groupsByName.Values;

        public string DefaultGroupResourceNameOrNull
        {
            get { return _defaultGroupResourceNameOrNull; }
        }

        public void Fill()
        {
            
            var groups = new List<ContactGroup>();

            ListContactGroupsResponse response;
            // TODO-GPA: es gibt ein executeasync
            // TODO-GPA: ListRequest hat ein Synctoken, mit den nur neue geholt werden können. Kann für das Batchentityrepository verwendet werden
            response = _apiOperationExecutor.Execute(s => s.ContactGroups.List().Execute());

            // TODO-GPA: ist die response eh nicht NULL, wenn keine Gruppen (mehr vorhanden sind)
            for (;;)
            {
                groups.AddRange(response.ContactGroups);
                if (response.NextPageToken != null)
                {
                    response = _apiOperationExecutor.Execute(s =>
                    {
                        var request = s.ContactGroups.List();
                        request.PageToken = response.NextPageToken;
                        return request.Execute();
                    });
                }
                else
                {
                    break;
                }
            }

            SetGroups(groups);
        }

        void SetGroups(IEnumerable<ContactGroup> existingGroups)
        {
            foreach (var group in existingGroups)
            {
                ContactGroup existingGroup;
                ContactGroup winningGroup;
                if (_groupsByName.TryGetValue(group.Name, out existingGroup))
                {
                    // TODO-GPA: ist das wirklich der String, oder nur der Name einer konstanten, die iwo definiert ist ?
                    if (existingGroup.GroupType == "SYSTEM_CONTACT_GROUP")
                    {
                        winningGroup = existingGroup;
                    }
                    else if (group.GroupType == "SYSTEM_CONTACT_GROUP")
                    {
                        winningGroup = group;
                    }
                    else
                    {
                        if (string.CompareOrdinal(group.ResourceName, existingGroup.ResourceName) > 0)
                            winningGroup = group;
                        else
                            winningGroup = existingGroup;
                    }
                }
                else
                {
                    winningGroup = group;
                }

                _groupsByName[winningGroup.Name] = winningGroup;

                if (IsDefaultGroup(winningGroup))
                    _defaultGroupResourceNameOrNull = winningGroup.ResourceName;
            }
        }

        public ContactGroup GetOrCreateGroup(string groupName)
        {
            ContactGroup group;
            if (!_groupsByName.TryGetValue(groupName, out group))
            {
                group = CreateGroup(groupName);
                _groupsByName.Add(groupName, group);
            }

            return group;
        }

        public bool IsDefaultGroupId(string id)
        {
            if (_defaultGroupResourceNameOrNull == null)
                return false;

            return StringComparer.InvariantCultureIgnoreCase.Compare(_defaultGroupResourceNameOrNull, id) == 0;
        }

        public bool IsDefaultGroup(ContactGroup group)
        {
            // TODO-GPA: ist das wirklich der String, oder nur der Name einer konstanten, die iwo definiert ist ?
            // TODO-GPA: ist die defualt gruppe immer eine system gruppe, oder reichts wenn der name gleich ist
            return group.GroupType == "SYSTEM_CONTACT_GROUP" &&
                StringComparer.InvariantCultureIgnoreCase.Compare(group.Name, "contacts") == 0;
        }

        ContactGroup CreateGroup(string name)
        {
            var groupRequest = new CreateContactGroupRequest()
            {
                ContactGroup = new ContactGroup()
            };
            groupRequest.ContactGroup.Name = name;

            return _apiOperationExecutor.Execute(s => s.ContactGroups.Create( groupRequest).Execute());
        }


        public void AddDefaultGroupToContact(Person contact)
        {
            if (_defaultGroupResourceNameOrNull == null)
                return;

            contact.Memberships.Add(
                new Membership
                {
                    ContactGroupMembership = new ContactGroupMembership
                    {
                        ContactGroupResourceName = _defaultGroupResourceNameOrNull
                    }
                });
        }
    }
}