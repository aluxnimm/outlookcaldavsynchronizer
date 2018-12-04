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
using System.IO;
using System.Text;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists.Sogo
{
  class SogoDistributionListRepository : CardDavEntityRepository<DistributionList, int, DistributionListSychronizationContext>
  {
    private const string NonAddressBookMemberValueName = "X-ADDRESSBOOKSERVER-MEMBER";

    public SogoDistributionListRepository(ICardDavDataAccess cardDavDataAccess, IEqualityComparer<string> versionComparer) : base(cardDavDataAccess, versionComparer)
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
      char [] escapechars = { ',', '\\', ';', '\r', '\n' };

      var builder = new StringBuilder();
      builder.AppendLine("BEGIN:VLIST");
      builder.AppendLine("VERSION:1.0");
      builder.Append("UID:");
      builder.AppendLine(vcard.Uid);
      builder.Append("FN:");
      builder.AppendLine(vCardStandardWriter.EncodeEscaped(vcard.Name,escapechars));
      if (!string.IsNullOrEmpty(vcard.Description))
      {
        builder.Append("DESCRIPTION:");
        builder.AppendLine(vCardStandardWriter.EncodeEscaped(vcard.Description.Replace("\r\n", "\n"),escapechars));
      }
      if (!string.IsNullOrEmpty(vcard.Nickname))
      {
        builder.Append("NICKNAME:");
        builder.AppendLine(vCardStandardWriter.EncodeEscaped(vcard.Nickname, escapechars));
      }
      foreach (var member in vcard.Members)
      {
        builder.Append("CARD;EMAIL=");
        builder.Append(member.EmailAddress);
        builder.Append(";FN=");
        builder.Append(vCardStandardWriter.EncodeEscaped(member.DisplayName, escapechars));
        builder.Append(":");
        builder.AppendLine(member.ServerFileName);
      }

      foreach (var member in vcard.NonAddressBookMembers)
      {
        builder.Append(NonAddressBookMemberValueName + ";CN=");
        builder.Append(vCardStandardWriter.EncodeEscaped(member.DisplayName, escapechars));
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
      var cardReader = new vCardStandardReader();
      vcard = new DistributionList();

      using (var reader = new StringReader(vcardData))
      {
        vCardProperty property;
        do
        {
          property = cardReader.ReadProperty(reader);
          if (!string.IsNullOrEmpty(property?.Name))
          { 
            var propNameToProcess = property.Name.ToUpperInvariant();

            switch (propNameToProcess)
            {
              case "UID":
                vcard.Uid = property.ToString();
                break;
              case "FN":
                vcard.Name = property.ToString();
                break;
              case "DESCRIPTION":
                vcard.Description = property.ToString();
                break;
              case "NICKNAME":
                vcard.Nickname = property.ToString();
                break;
              case "CARD":
                if (property.Value != null)
                {
                  var emailAddress = property.Subproperties.GetValue("EMAIL");
                  var displayName = property.Subproperties.GetValue("FN");
                  vcard.Members.Add(new KnownDistributionListMember(emailAddress, displayName, property.Value.ToString()));
                }
                break;
              case NonAddressBookMemberValueName:
                if (property.Value != null)
                {
                  var displayName = property.Subproperties.GetValue("CN");
                  string emailAddress = null;
                  var value = property.Value.ToString();
                  if (!string.IsNullOrEmpty(value))
                  {
                    if (value.StartsWith("mailto:", StringComparison.InvariantCultureIgnoreCase))
                    {
                      emailAddress = value.Substring(7); //skip mailto:
                    }
                  }
                  vcard.NonAddressBookMembers.Add(new DistributionListMember(emailAddress, displayName));
                }
                break;
            }
          }
        } while (property != null);
      }
      return true;
    }
  }
}