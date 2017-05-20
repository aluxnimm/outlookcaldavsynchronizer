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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase.ComWrappers;
using CalDavSynchronizer.Scheduling;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public class TestComponentContainer : IDisposable
  {
    public ComponentContainer ComponentContainer;
    public SynchronizerFactory SynchronizerFactory;
    public GeneralOptions GeneralOptions => new GeneralOptionsDataAccess ().LoadOptions ();
    private readonly TestComWrapperFactoryWrapper _testComWrapperFactoryWrapper;
    public Application Application { get; private set; }
    
    public TestComponentContainer()
    {
      Application = new Application();
      Application.Session.Logon();

      _testComWrapperFactoryWrapper = new TestComWrapperFactoryWrapper(new TestComWrapperFactory(null));
      ComponentContainer = new ComponentContainer(Application, new NullUiServiceFactory(), new InMemoryGeneralOptionsDataAccess(), _testComWrapperFactoryWrapper, new TestExceptionHandlingStrategy());
      SynchronizerFactory = ComponentContainer.GetSynchronizerFactory();
    }

    public void SetMaximumOpenItemsPerType(int? value)
    {
      _testComWrapperFactoryWrapper.SetInner(new TestComWrapperFactory(value));
    }

    public static Options GetOptions (string profileName)
    {
      var applicationDataDirectory = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

      var optionsDataAccess = new OptionsDataAccess (
        Path.Combine (
          applicationDataDirectory,
          ComponentContainer.GetOrCreateConfigFileName (applicationDataDirectory, "Outlook")
        ));

      var options = optionsDataAccess.Load ().Single (o => o.Name == profileName);
      return options;
    }
    
    public void Dispose()
    {
      try
      {
        Application.Session.Logoff();
      }
      finally
      {
        Marshal.FinalReleaseComObject(Application);
        Application = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
      }
    }
    }
}