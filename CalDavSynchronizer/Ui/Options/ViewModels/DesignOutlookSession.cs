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
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
    class DesignOutlookSession : NameSpace
    {
        public Recipient CreateRecipient(string RecipientName)
        {
            throw new NotImplementedException();
        }

        public MAPIFolder GetDefaultFolder(OlDefaultFolders FolderType)
        {
            throw new NotImplementedException();
        }

        public MAPIFolder GetFolderFromID(string EntryIDFolder, object EntryIDStore)
        {
            throw new NotImplementedException();
        }

        public object GetItemFromID(string EntryIDItem, object EntryIDStore)
        {
            throw new NotImplementedException();
        }

        public Recipient GetRecipientFromID(string EntryID)
        {
            throw new NotImplementedException();
        }

        public MAPIFolder GetSharedDefaultFolder(Recipient Recipient, OlDefaultFolders FolderType)
        {
            throw new NotImplementedException();
        }

        public void Logoff()
        {
            throw new NotImplementedException();
        }

        public void Logon(object Profile, object Password, object ShowDialog, object NewSession)
        {
            throw new NotImplementedException();
        }

        public MAPIFolder PickFolder()
        {
            throw new NotImplementedException();
        }

        public void RefreshRemoteHeaders()
        {
            throw new NotImplementedException();
        }

        public void AddStore(object Store)
        {
            throw new NotImplementedException();
        }

        public void RemoveStore(MAPIFolder Folder)
        {
            throw new NotImplementedException();
        }

        public void Dial(object ContactItem)
        {
            throw new NotImplementedException();
        }

        public void AddStoreEx(object Store, OlStoreType Type)
        {
            throw new NotImplementedException();
        }

        public SelectNamesDialog GetSelectNamesDialog()
        {
            throw new NotImplementedException();
        }

        public void SendAndReceive(bool showProgressDialog)
        {
            throw new NotImplementedException();
        }

        public AddressEntry GetAddressEntryFromID(string ID)
        {
            throw new NotImplementedException();
        }

        public AddressList GetGlobalAddressList()
        {
            throw new NotImplementedException();
        }

        public Store GetStoreFromID(string ID)
        {
            throw new NotImplementedException();
        }

        public MAPIFolder OpenSharedFolder(string Path, object Name, object DownloadAttachments, object UseTTL)
        {
            throw new NotImplementedException();
        }

        public object OpenSharedItem(string Path)
        {
            throw new NotImplementedException();
        }

        public SharingItem CreateSharingItem(object Context, object Provider)
        {
            throw new NotImplementedException();
        }

        public bool CompareEntryIDs(string FirstEntryID, string SecondEntryID)
        {
            throw new NotImplementedException();
        }

        public ContactCard CreateContactCard(AddressEntry AddressEntry)
        {
            throw new NotImplementedException();
        }

        public Application Application
        {
            get { throw new NotImplementedException(); }
        }

        public OlObjectClass Class
        {
            get { throw new NotImplementedException(); }
        }

        public NameSpace Session
        {
            get { throw new NotImplementedException(); }
        }

        public object Parent
        {
            get { throw new NotImplementedException(); }
        }

        public Recipient CurrentUser
        {
            get { throw new NotImplementedException(); }
        }

        public Folders Folders
        {
            get { throw new NotImplementedException(); }
        }

        public string Type
        {
            get { throw new NotImplementedException(); }
        }

        public AddressLists AddressLists
        {
            get { throw new NotImplementedException(); }
        }

        public SyncObjects SyncObjects
        {
            get { throw new NotImplementedException(); }
        }

        public bool Offline
        {
            get { throw new NotImplementedException(); }
        }

        public object MAPIOBJECT
        {
            get { throw new NotImplementedException(); }
        }

        public OlExchangeConnectionMode ExchangeConnectionMode
        {
            get { throw new NotImplementedException(); }
        }

        public Accounts Accounts
        {
            get { throw new NotImplementedException(); }
        }

        public string CurrentProfileName
        {
            get { throw new NotImplementedException(); }
        }

        public Stores Stores
        {
            get { throw new NotImplementedException(); }
        }

        public Store DefaultStore
        {
            get { throw new NotImplementedException(); }
        }

        public Categories Categories
        {
            get { throw new NotImplementedException(); }
        }

        public string ExchangeMailboxServerName
        {
            get { throw new NotImplementedException(); }
        }

        public string ExchangeMailboxServerVersion
        {
            get { throw new NotImplementedException(); }
        }

        public string AutoDiscoverXml
        {
            get { throw new NotImplementedException(); }
        }

        public OlAutoDiscoverConnectionMode AutoDiscoverConnectionMode
        {
            get { throw new NotImplementedException(); }
        }

        public event NameSpaceEvents_OptionsPagesAddEventHandler OptionsPagesAdd;
        public event NameSpaceEvents_AutoDiscoverCompleteEventHandler AutoDiscoverComplete;

        protected virtual void OnOptionsPagesAdd(PropertyPages pages, MAPIFolder folder)
        {
            OptionsPagesAdd?.Invoke(pages, folder);
        }

        protected virtual void OnAutoDiscoverComplete()
        {
            AutoDiscoverComplete?.Invoke();
        }
    }
}