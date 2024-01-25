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

namespace CalDavSynchronizer
{
    public static class WebResourceUrls
    {
        public static Uri GlobalOptionsFile => new Uri("https://sourceforge.net/p/outlookcaldavsynchronizer/code/ci/master/tree/GlobalOptions.xml?format=raw");
        public static Uri SiteContainingNewestVersion => new Uri("https://sourceforge.net/projects/outlookcaldavsynchronizer/best_release.json");
        public static Uri LatestVersionZipFile => new Uri("https://sourceforge.net/projects/outlookcaldavsynchronizer/files/latest/download?source=files");
        public static Uri ReadMeFile => new Uri("https://sourceforge.net/p/outlookcaldavsynchronizer/code/ci/master/tree/README.md?format=raw");
        public static Uri ReadMeFileDownloadSite => new Uri("https://sourceforge.net/projects/outlookcaldavsynchronizer/files/README.md/download");
        public static Uri HelpSite => new Uri("https://caldavsynchronizer.org/documentation/");
        public static Uri DonationSite => new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PWA2N6P5WRSJJ&lc=US");
        public static Uri ProjectHomeSite => new Uri("https://caldavsynchronizer.org/");
    }
}