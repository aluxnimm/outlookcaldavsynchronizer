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
    string ServerUrl { get; set; }
    OlItemType? OutlookFolderType { get; }
    string EmailAddress { get; }
    ServerAdapterType ServerAdapterType { get; set; }

    IWebDavClient CreateWebDavClient ();
    IWebProxy GetProxyIfConfigured ();
  }
}
