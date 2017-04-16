using System;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.States;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  internal class OutlookDistListUpdateFromNewerToOlder
    : UpdateFromNewerToOlder<string, DateTime, DistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod ().DeclaringType);

    public OutlookDistListUpdateFromNewerToOlder (
      EntitySyncStateEnvironment<string, DateTime, DistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext> environment,
      IEntityRelationData<string, DateTime, WebResourceName, string> knownData,
      DateTime newA,
      string newB)
      : base (environment, knownData, newA, newB)
    {
    }

    protected override bool AIsNewerThanB
    {
      get
      {
        // Assume that no modification means, that the item is never modified. Therefore it must be new. 
        if (_bEntity.RevisionDate == null)
          return false;

        return _aEntity.Inner.LastModificationTime.ToUniversalTime () >= _bEntity.RevisionDate.Value;
      }
    }
  }
}