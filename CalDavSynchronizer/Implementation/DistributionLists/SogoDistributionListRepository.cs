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
using System.Text;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.ThoughtvCardWorkaround;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  class SogoDistributionListRepository : CardDavEntityRepository<DistributionList, int, DistributionListSychronizationContext>
  {
    private const string NonAddressBookMemberValueName = "X-ADDRESSBOOKSERVER-MEMBER";

    public SogoDistributionListRepository(ICardDavDataAccess cardDavDataAccess, IChunkedExecutor chunkedExecutor) : base(cardDavDataAccess, chunkedExecutor)
    {
    }

    protected override void SetUid(DistributionList entity, string uid)
    {
      entity.Uid = uid;
    }

    protected override string GetUid(DistributionList entity)
    {
      return entity.Uid;
    }

    protected override string Serialize(DistributionList vcard)
    {
      char [] escapechars = { '\\', ';', '\r', '\n' };

      var builder = new StringBuilder();
      builder.AppendLine("BEGIN:VLIST");
      builder.AppendLine("VERSION:1.0");
      builder.Append("UID:");
      builder.AppendLine(vcard.Uid);
      builder.Append("FN:");
      builder.AppendLine(vCardImprovedWriter.EncodeEscaped(vcard.Name,escapechars));
      if (!string.IsNullOrEmpty(vcard.Description))
      {
        builder.Append("DESCRIPTION:");
        builder.AppendLine(vCardImprovedWriter.EncodeEscaped(vcard.Description.Replace("\r\n", "\n"),escapechars));
      }
      if (!string.IsNullOrEmpty(vcard.Nickname))
      {
        builder.Append("NICKNAME:");
        builder.AppendLine(vCardImprovedWriter.EncodeEscaped(vcard.Nickname, escapechars));
      }
      foreach (var member in vcard.Members)
      {
        builder.Append("CARD;EMAIL=");
        builder.Append(member.EmailAddress);
        builder.Append(";FN=\"");
        builder.Append(vCardImprovedWriter.EncodeEscaped(member.DisplayName, escapechars));
        builder.Append("\":");
        builder.AppendLine(member.ServerFileName);
      }

      foreach (var member in vcard.NonAddressBookMembers)
      {
        builder.Append(NonAddressBookMemberValueName + ";CN=");
        builder.Append(vCardImprovedWriter.EncodeEscaped(member.DisplayName, escapechars));
        builder.Append(":mailto:");
        builder.AppendLine(member.EmailAddress);
      }
  
      builder.AppendLine("END:VLIST");
      return builder.ToString();
    }

    /*
BEGIN:VLIST
UID:4250-58658600-5-45D0678.vlf
VERSION:1.0
FN:Simpsons
CARD;EMAIL=homer@simpson.com;FN="Simpson, Homer":5228bf7b-f3a6-4c16-ae3e-90102d86ab43.vcf
CARD;EMAIL=homer@simpson.com;FN="Simpson, Marge":c520e3ad-3d19-4c2f-b093-3f17736bce8e.vcf
END:VLIST 
     */

    protected override bool TryDeserialize(string vcardData, out DistributionList vcard, WebResourceName uriOfAddressbookForLogging, int deserializationThreadLocal, ILoadEntityLogger logger)
    {
      vcardData = vcardData.Replace("\r\n\t", string.Empty).Replace("\r\n ", string.Empty);

      vcard = new DistributionList();

      foreach (var contentLine in vcardData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
      {
        var valueStartIndex = contentLine.LastIndexOf(':') + 1;
        var value = contentLine.Substring(valueStartIndex);

        if (contentLine.StartsWith("UID"))
          vcard.Uid = value;
        else if (contentLine.StartsWith("FN"))
          vcard.Name = vCardStandardReader.DecodeEscaped(value);
        else if (contentLine.StartsWith("DESCRIPTION"))
          vcard.Description = vCardStandardReader.DecodeEscaped(value);
        else if (contentLine.StartsWith("NICKNAME"))
          vcard.Nickname = vCardStandardReader.DecodeEscaped(value);
        else if (contentLine.StartsWith("CARD"))
        {
          string displayName = null;
          string emailAddress = null;
          ParseMemberContentLineParameters(contentLine.Substring(0, valueStartIndex-1), out emailAddress, out displayName);
          vcard.Members.Add(new KnownDistributionListMember(emailAddress, displayName, value));
        }
        else if (contentLine.StartsWith(NonAddressBookMemberValueName))
        {
          string displayName = null;
          string emailAddress = value;
          var contentWithoutMailto = contentLine.Substring(0, valueStartIndex - 8); // substract :mailto:
          
          ParseXAddressBookServerMemberContentLineParameters(contentWithoutMailto, out displayName);
          vcard.NonAddressBookMembers.Add(new DistributionListMember(emailAddress, displayName));
        }
      }

      return true;
    }

    private static void ParseMemberContentLineParameters(string contentLineWithoutValue,out string emailAddress, out string displayName)
    {
      emailAddress = null;
      displayName = null;

      var parameters = contentLineWithoutValue.Split(";", '\\');
      foreach (var parameter in parameters)
      {
        if (parameter.StartsWith("EMAIL="))
          emailAddress = parameter.Substring(6);
        if (parameter.StartsWith("FN="))
          displayName = vCardStandardReader.DecodeEscaped(parameter.Substring(3));
      }
    }
    private static void ParseXAddressBookServerMemberContentLineParameters (string contentLineWithoutValue, out string displayName)
    {
      displayName = null;

      var parameters = contentLineWithoutValue.Split(";", '\\');
      foreach (var parameter in parameters)
      {
        if (parameter.StartsWith("CN="))
          displayName = vCardStandardReader.DecodeEscaped(parameter.Substring(3));
      }
    }

  }
}