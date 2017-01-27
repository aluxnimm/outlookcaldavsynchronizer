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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;
using Exception = System.Exception;


namespace CalDavSynchronizer.Implementation.DistributionLists
{
  public class DistListEntityMapper : IEntityMapper<GenericComObjectWrapper<DistListItem>, DistributionList, DistributionListSychronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public Task<DistributionList> Map1To2(GenericComObjectWrapper<DistListItem> source, DistributionList target, IEntityMappingLogger logger, DistributionListSychronizationContext context)
    {
      target.Members.Clear();
      target.NonAddressBookMembers.Clear();
      target.Name = source.Inner.DLName;
      target.Description = source.Inner.Body;

      try
      {
        using (var userPropertiesWrapper = GenericComObjectWrapper.Create(source.Inner.UserProperties))
        {
          using (var userProperty = GenericComObjectWrapper.Create(userPropertiesWrapper.Inner.Find("NICKNAME")))
          {
            target.Nickname = userProperty.Inner?.Value.ToString();
          }
        }
      }
      catch (COMException ex)
      {
        s_logger.Warn ("Can't access UserProperty of Distribution List!", ex);
        logger.LogMappingWarning ("Can't access UserProperty of Distribution List!", ex);
      }

      for (int i = 1; i <= source.Inner.MemberCount; i++)
      {
        try
        {
          using (var recipientWrapper = GenericComObjectWrapper.Create(source.Inner.GetMember(i)))
          {
            var serverFileName = context.GetServerFileNameByEmailAddress(recipientWrapper.Inner.Address);
            if (serverFileName != null)
            {
              var distributionListMember = new KnownDistributionListMember(recipientWrapper.Inner.Address, recipientWrapper.Inner.Name, serverFileName);
              target.Members.Add(distributionListMember);
            }
            else
            {
              var distributionListMember = new DistributionListMember(recipientWrapper.Inner.Address, recipientWrapper.Inner.Name);
              target.NonAddressBookMembers.Add(distributionListMember);
            }
          }
        }
        catch (COMException ex)
        {
          s_logger.Warn("Can't access member of Distribution List!", ex);
          logger.LogMappingWarning("Can't access member of Distribution List!", ex);
        }
      }

      return Task.FromResult(target);
    }

    public Task<GenericComObjectWrapper<DistListItem>> Map2To1(DistributionList source, GenericComObjectWrapper<DistListItem> target, IEntityMappingLogger logger, DistributionListSychronizationContext context)
    {

      var outlookMembersByAddress = new Dictionary<string, GenericComObjectWrapper<Recipient>>(StringComparer.InvariantCultureIgnoreCase);

      target.Inner.DLName = source.Name;
      if (!string.IsNullOrEmpty(source.Description))
        target.Inner.Body = source.Description;

      try
      {
        using (var userPropertiesWrapper = GenericComObjectWrapper.Create(target.Inner.UserProperties))
        {
          using (var userProperty = GenericComObjectWrapper.Create(userPropertiesWrapper.Inner.Find("NICKNAME")))
          {
            if (userProperty.Inner != null)
            {
              userProperty.Inner.Value = source.Nickname;
            }
            else if (!string.IsNullOrEmpty(source.Nickname))
            {
              using (var newUserProperty = GenericComObjectWrapper.Create(userPropertiesWrapper.Inner.Add("NICKNAME", OlUserPropertyType.olText, true)))
              {
                newUserProperty.Inner.Value = source.Nickname;
              }
            }
          }
        }
      }
      catch (COMException ex)
      {
        s_logger.Warn("Can't access UserProperty of Distribution List!", ex);
        logger.LogMappingWarning("Can't access UserProperty of Distribution List!", ex);
      }

      try
      {
        for (int i = 1; i <= target.Inner.MemberCount; i++)
        {
          var recipientWrapper = GenericComObjectWrapper.Create(target.Inner.GetMember(i));
          if (!string.IsNullOrEmpty(recipientWrapper.Inner?.Address) &&
              !outlookMembersByAddress.ContainsKey(recipientWrapper.Inner.Address))
          {
            outlookMembersByAddress.Add(recipientWrapper.Inner.Address, recipientWrapper);
          }
          else
          {
            recipientWrapper.Dispose();
          }
        }

        foreach (var sourceMember in source.Members.Concat(source.NonAddressBookMembers))
        {
          GenericComObjectWrapper<Recipient> existingRecipient;
          if (!string.IsNullOrEmpty(sourceMember.EmailAddress) &&
              outlookMembersByAddress.TryGetValue(sourceMember.EmailAddress, out existingRecipient))
          {
            outlookMembersByAddress.Remove(sourceMember.EmailAddress);
            existingRecipient.Dispose();
          }
          else
          {
            var recipientStringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(sourceMember.DisplayName))
            {
              if (!string.IsNullOrEmpty(sourceMember.EmailAddress))
                recipientStringBuilder.Append(sourceMember.EmailAddress);
            }
            else
            {
              recipientStringBuilder.Append(sourceMember.DisplayName);
              if (  sourceMember.EmailAddress != sourceMember.DisplayName &&
                    !string.IsNullOrEmpty(sourceMember.EmailAddress) && 
                    !sourceMember.DisplayName.EndsWith(")")
                 )
              {
                recipientStringBuilder.Append(" (");
                recipientStringBuilder.Append(sourceMember.EmailAddress);
                recipientStringBuilder.Append(")");
              }
            }

            if (recipientStringBuilder.Length > 0)
            {
              var recipient = context.OutlookSession.CreateRecipient(recipientStringBuilder.ToString());
              recipient.Resolve();
              target.Inner.AddMember(recipient);
            }
          }
        }

        foreach (var existingRecipient in outlookMembersByAddress.ToArray())
        {
          target.Inner.RemoveMember(existingRecipient.Value.Inner);
          outlookMembersByAddress.Remove(existingRecipient.Key);
        }
      }
      catch (COMException ex)
      {
        s_logger.Warn("Can't access member of Distribution List!", ex);
        logger.LogMappingWarning("Can't access member of Distribution List!", ex);
      }
      finally
      {
        foreach (var existingRecipient in outlookMembersByAddress.Values)
        {
          existingRecipient.Dispose();
        }
      }
      return Task.FromResult(target);
    }
  }
}