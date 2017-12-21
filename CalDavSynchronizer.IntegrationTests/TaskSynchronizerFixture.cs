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
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using DDay.iCal;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{

  class TaskSynchronizerFixture
  {
    private TestComponentContainer _testComponentContainer;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }
    
  
    [TestCase(true)]
    [TestCase(false)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task CreateOutlookEntity_ExceptionOccurs_DoesNotLeaveEmptyEntityInRepository(bool saveAndReload)
    {
      var options = _testComponentContainer.TestOptionsFactory.CreateSogoTasks();
      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      bool exceptionCatched = false;

      var exception = new Exception("bla");
      try
      {
        await synchronizer.Components.OutlookRepository.Create(
          w =>
          {
            if(saveAndReload)
              w.SaveAndReload();
            throw exception;
          },
          0);
      }
      catch (Exception x)
      {
        if(ReferenceEquals(x, exception))
          exceptionCatched = true;
      }

      Assert.That(exceptionCatched, Is.EqualTo(true));

      Assert.That(
        (await synchronizer.Outlook.GetAllEntities()).Count(),
        Is.EqualTo(0));
    }

  
    private async Task<TaskTestSynchronizer> CreateSynchronizer(Options options)
    {
      var synchronizer = new TaskTestSynchronizer(options, _testComponentContainer);
      await synchronizer.Initialize();
      return synchronizer;
    }
  }
}
