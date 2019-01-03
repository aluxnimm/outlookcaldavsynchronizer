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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists.VCard
{
  public class UidDistListEntityMapper : DistListEntityMapperBase
  {
    protected override vCardMember CreateVCardMemberOrNull(GenericComObjectWrapper<Recipient> recipientWrapper, string nameWithoutEmail, DistributionListSychronizationContext context, IEntitySynchronizationLogger synchronizationLogger, ILog logger)
    {
      var uid = context.GetUidByEmailAddress(recipientWrapper.Inner.Address);
      if (uid == null)
      {
        var logMessage = $"Did not find Uid of EmailAddress '{recipientWrapper.Inner.Address}'. Member won't be added to contact group";
        logger.WarnFormat(logMessage);
        synchronizationLogger.LogWarning(logMessage);
      }

      var targetMember = new vCardMember();
      targetMember.Uid = uid;
      return targetMember;
    }

    protected override IEnumerable<DistributionListMember> GetMembers(vCard source, DistributionListSychronizationContext context, IEntitySynchronizationLogger synchronizationLogger, ILog logger)
    {
      foreach(var member in source.Members)
      {
        (var contactWrapper, var emailAddress) = context.GetContactByUidOrNull(member.Uid, synchronizationLogger, logger);
        if (contactWrapper != null)
        {
          DistributionListMember distributionListMember;
          using (contactWrapper)
          {
            distributionListMember = new DistributionListMember(emailAddress, contactWrapper.Inner.FullName);
          }
          yield return distributionListMember;
        }
      }
    }
  }
}