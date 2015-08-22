// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace CalDavSynchronizer.AutomaticUpdates
{
  internal class AvailableVersionService : IAvailableVersionService
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public Version GetVersionOfDefaultDownload ()
    {
      var client = new WebClient();
      var site = client.DownloadString (new Uri ("https://sourceforge.net/projects/outlookcaldavsynchronizer/files/"));

      var match = Regex.Match (site, @"OutlookCalDavSynchronizer-(?<Major>\d+).(?<Minor>\d+).(?<Build>\d+).zip");

      if (match.Success)
      {
        var availableVersion = new Version (
            int.Parse (match.Groups["Major"].Value),
            int.Parse (match.Groups["Minor"].Value),
            int.Parse (match.Groups["Build"].Value));

        return availableVersion;
      }
      else
      {
        return null;
      }
    }

    public string GetWhatsNewNoThrow (Version oldVersion, Version newVersion)
    {
      try
      {
        var client = new WebClient();
        var readme = client.DownloadString (new Uri ("http://sourceforge.net/p/outlookcaldavsynchronizer/code/ci/master/tree/README.md?format=raw"));

        var start = Find (readme, newVersion);
        var end = Find (readme, oldVersion);

        if (start == -1 || end == -1)
        {
          if (start == -1)
            s_logger.ErrorFormat ("Did not find Version '{0}' in readme.md", newVersion);

          if (end == -1)
            s_logger.ErrorFormat ("Did not find Version '{0}' in readme.md", oldVersion);

          return "Did not find any news.";
        }

        return readme.Substring (start, end - start);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        return ("Error while trying to fetch the news.\r\nPlease see logfile for details.");
      }
    }


    private static int Find (string contents, Version version)
    {
      var match = Regex.Match (contents, string.Format (@"####\s*{0}\s*####", version.ToString (3)));
      return match.Success ? match.Index : -1;
    }

    public string DownloadLink
    {
      get { return "https://sourceforge.net/projects/outlookcaldavsynchronizer/files/latest/download?source=files"; }
    }
  }
}