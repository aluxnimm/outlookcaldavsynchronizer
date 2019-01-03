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

using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;


namespace CalDavSynchronizer.Implementation.DistributionLists.Sogo
{
  public class SogoDistListEntityMapper : IEntityMapper<IDistListItemWrapper, DistributionList, DistributionListSychronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public Task<DistributionList> Map1To2(IDistListItemWrapper source, DistributionList target, IEntitySynchronizationLogger logger, DistributionListSychronizationContext context)
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
        logger.LogWarning ("Can't access UserProperty of Distribution List!", ex);
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
              var nameWithoutEmail = OutlookUtility.RemoveEmailFromName(recipientWrapper.Inner);
              var distributionListMember = new KnownDistributionListMember(recipientWrapper.Inner.Address, nameWithoutEmail, serverFileName);
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
          logger.LogWarning("Can't access member of Distribution List!", ex);
        }
      }

      return Task.FromResult(target);
    }

    public Task<IDistListItemWrapper> Map2To1(DistributionList source, IDistListItemWrapper target, IEntitySynchronizationLogger logger, DistributionListSychronizationContext context)
    {
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
        logger.LogWarning("Can't access UserProperty of Distribution List!", ex);
      }

      CommonEntityMapper.MapDistListMembers2To1 (source.Members.Concat (source.NonAddressBookMembers), target, logger, context);
      return Task.FromResult(target);
    }
  }
}