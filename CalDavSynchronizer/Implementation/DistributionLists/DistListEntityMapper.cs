using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  public class DistListEntityMapper : IEntityMapper<GenericComObjectWrapper<DistListItem>, vCard, DistributionListSychronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    public Task<vCard> Map1To2(GenericComObjectWrapper<DistListItem> source, vCard target, IEntityMappingLogger logger, DistributionListSychronizationContext context)
    {
      target.Members.Clear ();
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
            var nameWithoutEmail = Regex.Replace(recipientWrapper.Inner.Name, " \\([^()]*\\)$", string.Empty);
            var targetMember = new vCardMember();
            targetMember.EmailAddress = recipientWrapper.Inner.Address;
            targetMember.DisplayName = nameWithoutEmail;
            target.Members.Add(targetMember);
          }
        }
        catch (COMException ex)
        {
          s_logger.Warn ("Can't access member of Distribution List!", ex);
          logger.LogMappingWarning ("Can't access member of Distribution List!", ex);
        }
      }

      return Task.FromResult (target);
    }

    public Task<GenericComObjectWrapper<DistListItem>> Map2To1(vCard source, GenericComObjectWrapper<DistListItem> target, IEntityMappingLogger logger, DistributionListSychronizationContext context)
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

      CommonEntityMapper.MapDistListMembers2To1(source.Members.Select(v => new DistributionListMember(v.EmailAddress, v.DisplayName)), target, logger, context);

      return Task.FromResult (target);
    }


  }
}