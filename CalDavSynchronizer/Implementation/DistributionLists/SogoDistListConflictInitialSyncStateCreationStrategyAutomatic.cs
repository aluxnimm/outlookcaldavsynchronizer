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
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;
using GenSync.Synchronization.States;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  internal class SogoDistListConflictInitialSyncStateCreationStrategyAutomatic
      : ConflictInitialSyncStateCreationStrategyAutomatic<string, DateTime, GenericComObjectWrapper<DistListItem>, WebResourceName, string, DistributionList, DistributionListSychronizationContext>
  {
    public SogoDistListConflictInitialSyncStateCreationStrategyAutomatic(EntitySyncStateEnvironment<string, DateTime, GenericComObjectWrapper<DistListItem>, WebResourceName, string, DistributionList, DistributionListSychronizationContext> environment)
        : base (environment)
    {
    }

    protected override IEntitySyncState<string, DateTime, GenericComObjectWrapper<DistListItem>, WebResourceName, string, DistributionList, DistributionListSychronizationContext> Create_FromNewerToOlder (IEntityRelationData<string, DateTime, WebResourceName, string> knownData, DateTime newA, string newB)
    {
      return new OutlookSogoDistListUpdateFromNewerToOlder (
          _environment,
          knownData,
          newA,
          newB
          );
    }
  }
}