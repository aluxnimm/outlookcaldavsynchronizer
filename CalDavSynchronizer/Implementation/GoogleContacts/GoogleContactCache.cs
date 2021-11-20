// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
    public class GoogleContactCache : IGoogleContactCache
    {
        private readonly Dictionary<string, Person> _contactsById;
        private readonly IGoogleApiOperationExecutor _apiOperationExecutor;
        private readonly string _userName;
        private readonly int _readChunkSize;

        public GoogleContactCache(IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer, string userName, int readChunkSize)
        {
            if (apiOperationExecutor == null)
                throw new ArgumentNullException(nameof(apiOperationExecutor));
            if (contactIdComparer == null)
                throw new ArgumentNullException(nameof(contactIdComparer));
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Argument is null or empty", nameof(userName));

            _apiOperationExecutor = apiOperationExecutor;
            _userName = userName;
            _readChunkSize = readChunkSize;
            _contactsById = new Dictionary<string, Person>(contactIdComparer);
        }

        public void Fill(string defaultGroupIdOrNull)
        {
            // TODO-GPA: operationexecutor must be reworked, since the retry must of course be whjen the webrequest is executed
            var listRequest = _apiOperationExecutor.Execute(f => f.People.Connections.List("people/me"));

            // TODO-GPA: wozu wurde das gebraucht ?
            //if (defaultGroupIdOrNull != null)
            //    query.Group = defaultGroupIdOrNull;

            listRequest.PersonFields = "names, nicknames, occupations, organizations, phoneNumbers";

            do
            {
                // TODO-GPA switch to async
                var response = listRequest.Execute();

                foreach (var person in response.Connections)
                {
                    _contactsById[person.ResourceName] = person;
                }

                // TODO-GPA: Test if apging really works
                listRequest.PageToken = response.NextPageToken;

            } while (!string.IsNullOrEmpty(listRequest.PageToken));
        }

        public bool TryGetValue(string key, out Person value)
        {
            return _contactsById.TryGetValue(key, out value);
        }

        public Task<IEnumerable<EntityVersion<string, GoogleContactVersion>>> GetAllVersions()
        {
            var contacts = _contactsById.Values
                .Select(c => EntityVersion.Create(c.ResourceName, new GoogleContactVersion { ContactEtag = c.ETag }))
                .ToArray();
            return Task.FromResult<IEnumerable<EntityVersion<string, GoogleContactVersion>>>(contacts);
        }
    }
}