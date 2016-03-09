using System;
using System.Net;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options
{
  public class DesignCurrentOptions : ICurrentOptions
  {
    public SynchronizationMode SynchronizationMode
    {
      get { throw new NotImplementedException(); }
    }

    public string SynchronizationModeDisplayName
    {
      get { throw new NotImplementedException(); }
    }

    public string ServerUrl
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IWebDavClient CreateWebDavClient ()
    {
      throw new NotImplementedException();
    }

    public IWebProxy GetProxyIfConfigured ()
    {
      throw new NotImplementedException();
    }

    public ICalDavDataAccess CreateCalDavDataAccess ()
    {
      throw new NotImplementedException();
    }

    public OlItemType? OutlookFolderType
    {
      get { throw new NotImplementedException(); }
    }

    public string EmailAddress
    {
      get { throw new NotImplementedException(); }
    }

    public ServerAdapterType ServerAdapterType
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }
  }
}