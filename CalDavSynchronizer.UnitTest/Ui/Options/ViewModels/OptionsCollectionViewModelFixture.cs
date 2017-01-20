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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.ViewModels;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using Rhino.Mocks;

namespace CalDavSynchronizer.UnitTest.Ui.Options.ViewModels
{
  [TestFixture]
  public class OptionsCollectionViewModelFixture
  {
    private IUiService _uiServiceStub;
    private OptionsCollectionViewModel _viewModel;
    private IOptionTasks _optionTasksStub;

    [SetUp]
    public void SetUp()
    {
      _uiServiceStub = MockRepository.GenerateStub<IUiService>();

      // see http://stackoverflow.com/questions/3444581/mocking-com-interfaces-using-rhino-mocks
      // Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add (typeof (TypeIdentifierAttribute));

      _optionTasksStub = MockRepository.GenerateStub<IOptionTasks>();
      _viewModel = new OptionsCollectionViewModel(
        false,
        id => @"A:\bla",
        _uiServiceStub,
        _optionTasksStub,
        p => new OptionsViewModelFactory(
          p,
          MockRepository.GenerateStub<IOutlookAccountPasswordProvider>(),
          new string[0],
          _optionTasksStub,
          NullSettingsFaultFinder.Instance,
          new GeneralOptions(),
          new ViewOptions (false)),
        new ViewOptions (false));
    }

    [Test]
    public void AddNewGenericProfile()
    {
      _uiServiceStub.Stub(_ => _.QueryProfileType()).Return(Contracts.ProfileType.Generic);

      _viewModel.SetOptionsCollection(new Contracts.Options[0]);
      _viewModel.AddCommand.Execute(null);

      var profile = (GenericOptionsViewModel) _viewModel.Options[0];
      profile.Name = "p1";
      profile.Model.CalenderUrl = "http://caldav.com/";
      
      _optionTasksStub.Stub(_ => _.PickFolderOrNull()).Return(new OutlookFolderDescriptor("folderId", "storeId", OlItemType.olAppointmentItem, "cal"));
      _optionTasksStub.Stub(_ => _.GetFolderAccountNameOrNull("storeId")).Return("accountName");
      profile.OutlookFolderViewModel.SelectFolderCommand.Execute(null);


      var closeRequestedHandlerMock = MockRepository.GenerateStrictMock<EventHandler<CloseEventArgs>>();
      closeRequestedHandlerMock.Expect(_ => _(Arg.Is(_viewModel), Arg<CloseEventArgs>.Matches(e => e.ShouldSaveNewOptions)));
      _viewModel.CloseRequested += closeRequestedHandlerMock;
      _viewModel.CloseCommand.Execute(true);
      closeRequestedHandlerMock.VerifyAllExpectations();

      var options = _viewModel.GetOptionsCollection().Single();

      Assert.That(options.Name, Is.EqualTo("p1"));
      Assert.That(options.CalenderUrl, Is.EqualTo("http://caldav.com/"));
      Assert.That(options.OutlookFolderEntryId, Is.EqualTo("folderId"));
      Assert.That(options.OutlookFolderStoreId, Is.EqualTo("storeId"));
      Assert.That(options.OutlookFolderAccountName, Is.EqualTo("accountName"));
      Assert.That(options.ServerAdapterType, Is.EqualTo(ServerAdapterType.WebDavHttpClientBased));
    }
  }
}
