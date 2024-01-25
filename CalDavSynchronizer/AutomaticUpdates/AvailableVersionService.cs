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
using System.Text.RegularExpressions;
using log4net;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Globalization;
using Newtonsoft.Json.Linq;

namespace CalDavSynchronizer.AutomaticUpdates
{
    internal class AvailableVersionService : IAvailableVersionService
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

        public Version GetVersionOfDefaultDownload()
        {
            string site;

            using (var client = HttpUtility.CreateWebClient())
            {
                site = client.DownloadString(WebResourceUrls.SiteContainingNewestVersion);
            }

            var bestReleaseJObject = JObject.Parse(site);

            if (!(bestReleaseJObject["release"]?["filename"] is JValue fileNameValue))
                return null;

            var match = Regex.Match(fileNameValue.Value<string>(), @"/(?<Major>\d+).(?<Minor>\d+).(?<Build>\d+)/");

            if (match.Success)
            {
                var availableVersion = new Version(
                    int.Parse(match.Groups["Major"].Value),
                    int.Parse(match.Groups["Minor"].Value),
                    int.Parse(match.Groups["Build"].Value));

                return availableVersion;
            }
            else
            {
                return null;
            }
        }

        public string GetWhatsNewNoThrow(Version oldVersion, Version newVersion)
        {
            try
            {
                string readme;

                using (var client = HttpUtility.CreateWebClient())
                {
                    readme = client
                        .DownloadString(WebResourceUrls.ReadMeFile)
                        .Replace("\n", Environment.NewLine).Replace("\t", "   ");
                }

                var start = Find(readme, newVersion);
                var end = Find(readme, oldVersion);

                if (start == -1 || end == -1)
                {
                    if (start == -1)
                        s_logger.ErrorFormat("Did not find Version '{0}' in readme.md", newVersion);

                    if (end == -1)
                        s_logger.ErrorFormat("Did not find Version '{0}' in readme.md", oldVersion);

                    return Strings.Get($"Did not find any news.");
                }

                return readme.Substring(start, end - start);
            }
            catch (Exception x)
            {
                s_logger.Error(null, x);
                return Strings.Get($"Error while trying to fetch the news.\r\nPlease see logfile for details.");
            }
        }


        private static int Find(string contents, Version version)
        {
            var match = Regex.Match(contents, string.Format(@"####\s*{0}\s*####", version.ToString(3)));
            return match.Success ? match.Index : -1;
        }

        public Uri DownloadLink
        {
            get { return WebResourceUrls.LatestVersionZipFile; }
        }
    }
}