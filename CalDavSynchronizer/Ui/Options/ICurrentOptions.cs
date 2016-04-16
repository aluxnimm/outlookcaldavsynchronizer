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
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using Microsoft.Office.Interop.Outlook;
using System.Net;

namespace CalDavSynchronizer.Ui.Options
{
  public interface ICurrentOptions
  {
    SynchronizationMode SynchronizationMode { get; }
    string SynchronizationModeDisplayName { get; }
    OlItemType? OutlookFolderType { get; }
    event EventHandler OutlookFolderTypeChanged;
    string EmailAddress { get; }
    ServerAdapterType ServerAdapterType { get; }

    IWebDavClient CreateWebDavClient ();
    IWebProxy GetProxyIfConfigured ();
    ICalDavDataAccess CreateCalDavDataAccess ();
  }
}
