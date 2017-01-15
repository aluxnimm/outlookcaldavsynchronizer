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
using System.Text.RegularExpressions;

namespace CalDavSynchronizer.ThoughtvCardWorkaround
{
  public static class ContactDataPreprocessor
  {
 
    public static string FixPhoto (string vcardData)
    {
      // Remove X-ABCROP-RECTANGLE since the Deserializer can't parse it
      return Regex.Replace (vcardData, "PHOTO(;.*?)?;X-ABCROP-RECTANGLE(.*?)(;.*?)?:", m => "PHOTO" + (m.Groups[1].Success ? m.Groups[1].Value : string.Empty) + (m.Groups[3].Success ? m.Groups[3].Value : string.Empty)+":" , RegexOptions.IgnoreCase | RegexOptions.Singleline);
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

    public static string NormalizeLineBreaks (string vcardData)
    {
      // Certain iCal providers like Open-Xchange deliver their data with unexpected linebreaks
      // which causes the parser to fail. This can be fixed by normalizing the unexpected \r\r\n to \r\n
      return vcardData.Replace ("\r\r\n", "\r\n");
    }
  }
}
