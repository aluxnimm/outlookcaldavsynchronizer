//// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
//// Copyright (c) 2015 Gerhard Zehetbauer
//// Copyright (c) 2015 Alexander Nimmervoll
//// 
//// This program is free software: you can redistribute it and/or modify
//// it under the terms of the GNU Affero General Public License as
//// published by the Free Software Foundation, either version 3 of the
//// License, or (at your option) any later version.
//// 
//// This program is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU Affero General Public License for more details.
//// 
//// You should have received a copy of the GNU Affero General Public License
//// along with this program.  If not, see <http://www.gnu.org/licenses/>.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GenSync;
//using Google.Contacts;
//using Google.GData.Client;
//using Google.GData.Contacts;

//namespace CalDavSynchronizer.Implementation.GoogleContacts
//{
//    public class GoogleContactCache : IGoogleContactCache
//    {
//        private readonly Dictionary<string, Contact> _contactsById;
//        private readonly IGoogleApiOperationExecutor _apiOperationExecutor;
//        private readonly string _userName;
//        private readonly int _readChunkSize;

//        public GoogleContactCache(IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer, string userName, int readChunkSize)
//        {
//            if (apiOperationExecutor == null)
//                throw new ArgumentNullException(nameof(apiOperationExecutor));
//            if (contactIdComparer == null)
//                throw new ArgumentNullException(nameof(contactIdComparer));
//            if (String.IsNullOrEmpty(userName))
//                throw new ArgumentException("Argument is null or empty", nameof(userName));

//            _apiOperationExecutor = apiOperationExecutor;
//            _userName = userName;
//            _readChunkSize = readChunkSize;
//            _contactsById = new Dictionary<string, Contact>(contactIdComparer);
//        }

//        public void Fill(string defaultGroupIdOrNull)
//        {
//            var query = new ContactsQuery(ContactsQuery.CreateContactsUri(_userName, ContactsQuery.fullProjection));
//            query.StartIndex = 0;
//            query.NumberToRetrieve = _readChunkSize;

//            if (defaultGroupIdOrNull != null)
//                query.Group = defaultGroupIdOrNull;

//            for (
//                var contactsFeed = _apiOperationExecutor.Execute(f => f.Get<Contact>(query));
//                contactsFeed != null;
//                contactsFeed = _apiOperationExecutor.Execute(f => f.Get(contactsFeed, FeedRequestType.Next)))
//            {
//                foreach (Contact contact in contactsFeed.Entries)
//                {
//                    _contactsById[contact.Id] = contact;
//                }
//            }
//        }

//        public bool TryGetValue(string key, out Contact value)
//        {
//            return _contactsById.TryGetValue(key, out value);
//        }

//        public Task<IEnumerable<EntityVersion<string, GoogleContactVersion>>> GetAllVersions()
//        {
//            var contacts = _contactsById.Values
//                .Select(c => EntityVersion.Create(c.Id, new GoogleContactVersion {ContactEtag = c.ETag}))
//                .ToArray();
//            return Task.FromResult<IEnumerable<EntityVersion<string, GoogleContactVersion>>>(contacts);
//        }
//    }
//}