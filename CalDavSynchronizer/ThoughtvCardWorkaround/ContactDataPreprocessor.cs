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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thought.vCards;

namespace CalDavSynchronizer.ThoughtvCardWorkaround
{
  public static class ContactDataPreprocessor
  {

    public static string FixRevisionDate (string vcardData)
    {
      // Reformat REV attribute to use Z for UTC if not set (fixes Owncloud)
      var revMatch = Regex.Match (vcardData, "REV:(.*?)\\+00:00\r?\n");
      if (revMatch.Success)
      {
        return Regex.Replace (vcardData, "REV:(.*?)\\+00:00\r?\n", "REV:" + revMatch.Groups[1].Value + "Z\r\n");
      }
      else
      {
        return vcardData;
      }
    }
    public static string FixUrlType (string vcardData)
    {
      string fixedVcardData = vcardData;

      // Reformat URL type since the Deserializer only parses URL;HOME and URL;WORK
      fixedVcardData = fixedVcardData.Replace ("URL;TYPE=HOME", "URL;HOME");
      fixedVcardData = fixedVcardData.Replace ("URL;TYPE=WORK", "URL;WORK");
      fixedVcardData = fixedVcardData.Replace ("URL;TYPE=home", "URL;HOME");
      fixedVcardData = fixedVcardData.Replace ("URL;TYPE=work", "URL;WORK");
      return fixedVcardData;
    }

    public static string FixOrg (string vcardData)
    {
      // Reformat ORG attribute to split CompanyName and Department
      var orgMatch = Regex.Match (vcardData, @"ORG:(.*?)\\;\\;(.*?)\r?\n");
      if (orgMatch.Success)
      {
        return Regex.Replace (vcardData, @"ORG:(.*?)\\;\\;(.*?)\r?\n", "ORG:" + orgMatch.Groups[1].Value + ";" + orgMatch.Groups[2].Value+ "\r\n");
      }
      else
      {
        return vcardData;
      }
    }

    public static string FixBday (string vcardData)
    {
      // Reformat BDAY attribute to yyyy-MM-dd if included to work around a BUG in vCard Library
      var bDayMatch = Regex.Match (vcardData, "BDAY:(.*?)\r\n");
      if (bDayMatch.Success)
      {
        DateTime date;
        if (DateTime.TryParse (bDayMatch.Groups[1].Value, out date))
        {
          return Regex.Replace (vcardData, "BDAY:(.*?)\r\n", "BDAY:" + date.ToString ("yyyy-MM-dd") + "\r\n");
        }
      }
      return vcardData;
    }

    public static string FixNote (string vcardData, vCardStandardWriter writer)
    {
      // Reformat NOTE attribute since quoted-printable is deprecated
      var noteMatch = Regex.Match(vcardData, "NOTE;ENCODING=QUOTED-PRINTABLE:(.*?)\r\n");
      if (noteMatch.Success)
      {
        string decodedNote = vCardStandardReader.DecodeQuotedPrintable (noteMatch.Groups[1].Value).Replace("\r\n", "\n");
        return Regex.Replace (vcardData, "NOTE;ENCODING=QUOTED-PRINTABLE:(.*?)\r\n", writer.EncodeEscaped ("NOTE:" + decodedNote) + "\r\n");
      }
      return vcardData;
    }
  }
}
