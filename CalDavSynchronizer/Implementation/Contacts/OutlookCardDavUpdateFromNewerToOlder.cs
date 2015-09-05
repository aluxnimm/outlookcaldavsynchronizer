// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.States;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  internal class OutlookCardDavUpdateFromNewerToOlder
      : UpdateFromNewerToOlder<string, DateTime, GenericComObjectWrapper<ContactItem>, Uri, string, vCard>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public OutlookCardDavUpdateFromNewerToOlder (
        EntitySyncStateEnvironment<string, DateTime, GenericComObjectWrapper<ContactItem>, Uri, string, vCard> environment,
        IEntityRelationData<string, DateTime, Uri, string> knownData,
        DateTime newA,
        string newB)
        : base (environment, knownData, newA, newB)
    {
    }

    protected override bool AIsNewerThanB
    {
      get
      {
        // TODO:
        s_logger.ErrorFormat ("This method is not yet implemented and considers outlook version always as newer.");
        return true;

        // Assume that no modification means, that the item is never modified. Therefore it must be new. 
        //if (_bEntity.RevisionDate == null)
        //  return false;

        //return _aEntity.Inner.LastModificationTime.ToUniversalTime() >= _bEntity.RevisionDate.Value;
      }
    }
  }
}