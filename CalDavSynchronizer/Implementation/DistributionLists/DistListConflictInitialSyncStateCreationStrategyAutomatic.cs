using System;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;
using GenSync.Synchronization.States;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  internal class DistListConflictInitialSyncStateCreationStrategyAutomatic
    : ConflictInitialSyncStateCreationStrategyAutomatic<string, DateTime, IDistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext>
  {
    public DistListConflictInitialSyncStateCreationStrategyAutomatic (EntitySyncStateEnvironment<string, DateTime, IDistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext> environment)
      : base (environment)
    {
    }

    protected override IEntitySyncState<string, DateTime, IDistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext> Create_FromNewerToOlder (IEntityRelationData<string, DateTime, WebResourceName, string> knownData, DateTime newA, string newB)
    {
      return new OutlookDistListUpdateFromNewerToOlder (
        _environment,
        knownData,
        newA,
        newB
      );
    }
  }
}