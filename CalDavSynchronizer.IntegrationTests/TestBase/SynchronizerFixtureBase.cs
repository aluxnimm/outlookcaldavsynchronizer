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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Scheduling;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public class SynchronizerFixtureBase : IntegrationFixtureBase
  {
    protected ComponentContainer ComponentContainer;
    protected SynchronizerFactory SynchronizerFactory;
    protected GeneralOptions GeneralOptions => new GeneralOptionsDataAccess ().LoadOptions ();
    
    [SetUp]
    public void SetUp()
    {
      ComponentContainer = new ComponentContainer(Application, new NullUiServiceFactory(), new InMemoryGeneralOptionsDataAccess());
      SynchronizerFactory = ComponentContainer.GetSynchronizerFactory();
    }


    protected static Options GetOptions (string profileName)
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


  }
}