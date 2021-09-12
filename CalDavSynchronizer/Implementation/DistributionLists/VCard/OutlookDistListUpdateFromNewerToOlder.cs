using System;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.States;
using log4net;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists.VCard
{
    internal class OutlookDistListUpdateFromNewerToOlder
        : UpdateFromNewerToOlder<string, DateTime, IDistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext>
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

        public OutlookDistListUpdateFromNewerToOlder(
            EntitySyncStateEnvironment<string, DateTime, IDistListItemWrapper, WebResourceName, string, vCard, DistributionListSychronizationContext> environment,
            IEntityRelationData<string, DateTime, WebResourceName, string> knownData,
            DateTime newA,
            string newB)
            : base(environment, knownData, newA, newB)
        {
        }

        protected override DateTime ModificationTimeA => _aEntity.Inner.LastModificationTime.ToUniversalTime();
        protected override DateTime? ModificationTimeB => _bEntity.RevisionDate;
    }
}