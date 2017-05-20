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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.States;
using Google.Contacts;
using log4net;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  internal class GoogleContactUpdateFromNewerToOlder
      : UpdateFromNewerToOlder<string, DateTime, IContactItemWrapper, string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public GoogleContactUpdateFromNewerToOlder (
        EntitySyncStateEnvironment<string, DateTime, IContactItemWrapper, string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext> environment,
        IEntityRelationData<string, DateTime, string, GoogleContactVersion> knownData,
        DateTime newA,
        GoogleContactVersion newB)
        : base (environment, knownData, newA, newB)
    {
    }

    protected override bool AIsNewerThanB
    {
      get
      {
        return _aEntity.Inner.LastModificationTime.ToUniversalTime() >= _bEntity.Contact.Updated.ToUniversalTime();
      }
    }
  }
}