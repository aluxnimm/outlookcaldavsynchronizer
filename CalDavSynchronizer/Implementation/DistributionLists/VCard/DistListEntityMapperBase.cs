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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using GenSync.EntityMapping;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists.VCard
{
  public abstract class DistListEntityMapperBase : IEntityMapper<IDistListItemWrapper, vCard, DistributionListSychronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public Task<vCard> Map1To2(IDistListItemWrapper source, vCard target, IEntitySynchronizationLogger logger, DistributionListSychronizationContext context)
    {
      target.Members.Clear();
      target.FormattedName = source.Inner.DLName;
      target.FamilyName = source.Inner.DLName;

      target.AccessClassification = CommonEntityMapper.MapPrivacy1To2(source.Inner.Sensitivity);

      target.Categories.Clear();
      if (!string.IsNullOrEmpty(source.Inner.Categories))
      {
        Array.ForEach(
          source.Inner.Categories.Split(new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
          c => target.Categories.Add(c.Trim())
        );
      }

      target.Notes.Clear();
      if (!string.IsNullOrEmpty(source.Inner.Body))
      {
        target.Notes.Add(new vCardNote(source.Inner.Body));
      }

      for (int i = 1; i <= source.Inner.MemberCount; i++)
      {
        try
        {
          using (var recipientWrapper = GenericComObjectWrapper.Create(source.Inner.GetMember(i)))
          {
            var nameWithoutEmail = OutlookUtility.RemoveEmailFromName(recipientWrapper.Inner);
            var targetMember = CreateVCardMemberOrNull(recipientWrapper, nameWithoutEmail, context, logger, s_logger);
            if (targetMember != null)
              target.Members.Add(targetMember);
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

    protected abstract vCardMember CreateVCardMemberOrNull(GenericComObjectWrapper<Recipient> recipientWrapper, string nameWithoutEmail, DistributionListSychronizationContext context, IEntitySynchronizationLogger synchronizationLogger, ILog logger);

    public Task<IDistListItemWrapper> Map2To1(vCard source, IDistListItemWrapper target, IEntitySynchronizationLogger logger, DistributionListSychronizationContext context)
    {
      if (string.IsNullOrEmpty(source.FormattedName))
      {
        var name = new StringBuilder();
        name.Append(source.FamilyName);
        if (!string.IsNullOrEmpty(source.GivenName))
        {
          if (name.Length > 0)
            name.Append(",");
          name.Append(source.GivenName);
        }
        if (!string.IsNullOrEmpty(source.AdditionalNames))
        {
          if (name.Length > 0)
            name.Append(",");
          name.Append(source.AdditionalNames);
        }
        if (name.Length > 0)
        {
          target.Inner.DLName = name.ToString();
        }
      }
      else
      {
        target.Inner.DLName = source.FormattedName;
      }

      target.Inner.Sensitivity = CommonEntityMapper.MapPrivacy2To1(source.AccessClassification);

      if (source.Categories.Count > 0)
      {
        string[] categories = new string[source.Categories.Count];
        source.Categories.CopyTo(categories, 0);
        target.Inner.Categories = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, categories);
      }
      else
      {
        target.Inner.Categories = string.Empty;
      }

      if (source.Notes.Count > 0)
      {
        target.Inner.Body = source.Notes[0].Text;
      }
      else
      {
        target.Inner.Body = string.Empty;
      }

      CommonEntityMapper.MapDistListMembers2To1(GetMembers(source, context, logger, s_logger), target, logger, context);

      return Task.FromResult(target);
    }

    protected abstract IEnumerable<DistributionListMember> GetMembers(vCard source, DistributionListSychronizationContext context, IEntitySynchronizationLogger synchronizationLogger, ILog logger);
  }
}