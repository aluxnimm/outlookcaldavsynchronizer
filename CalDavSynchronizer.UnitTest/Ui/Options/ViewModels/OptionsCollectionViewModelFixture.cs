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
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.Models;
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
    private TestProfileRegistry _testProfileRegistry;

    [SetUp]
    public void SetUp()
    {
      _uiServiceStub = MockRepository.GenerateStub<IUiService>();

      // see http://stackoverflow.com/questions/3444581/mocking-com-interfaces-using-rhino-mocks
      // Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add (typeof (TypeIdentifierAttribute));

      _optionTasksStub = MockRepository.GenerateStub<IOptionTasks>();

      _testProfileRegistry = new TestProfileRegistry(_optionTasksStub);

      _viewModel = new OptionsCollectionViewModel(
        false,
        id => @"A:\bla",
        _uiServiceStub,
        _optionTasksStub,
        _testProfileRegistry,
        (_,t) => t.CreateModelFactory(null, null, null, null, null, null, null, null),
        new ViewOptions (false));
    }

    [Test]
    public void AddNewGenericProfile()
    {
      _uiServiceStub.Stub(_ => _.QueryProfileType(null)).IgnoreArguments().Return(_testProfileRegistry.AllTypes.First());

      _viewModel.SetOptionsCollection(new Contracts.Options[0]);
      _viewModel.AddCommand.Execute(null);

      var profile = (GenericOptionsViewModel) _viewModel.Options[0];
      profile.Name = "p1";
      profile.Model.CalenderUrl = "http://caldav.com/";

      _optionTasksStub.Stub(_ => _.PickFolderOrNull()).Return(new OutlookFolderDescriptor("folderId", "storeId", OlItemType.olAppointmentItem, "cal", 0));
      _optionTasksStub.Stub(_ => _.GetFolderAccountNameOrNull("storeId")).Return("accountName");
      profile.OutlookFolderViewModel.SelectFolderCommand.Execute(null);


      var closeRequestedHandlerMock = MockRepository.GenerateStrictMock<EventHandler<CloseEventArgs>>();
      closeRequestedHandlerMock.Expect(_ => _(Arg.Is(_viewModel), Arg<CloseEventArgs>.Matches(e => e.IsAcceptedByUser)));
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

    private class TestProfileRegistry : IProfileTypeRegistry
    {
      public TestProfileRegistry(IOptionTasks optionTasksStub)
      {
        AllTypes = new[] {new TestProfile(optionTasksStub) };
      }

      public IReadOnlyList<IProfileType> AllTypes { get; }
      public IProfileType DetermineType(Contracts.Options data)
      {
        return AllTypes.First();
      }
    }

    private class TestProfile : IProfileType, IProfileModelFactory
    {
      private readonly IOptionTasks _optionTasksStub;

      public TestProfile(IOptionTasks optionTasksStub)
      {
        _optionTasksStub = optionTasksStub;
      }

      public string Name => "TestProfile";
      public IProfileType ProfileType => this;

      public OptionsModel CreateNewModel()
      {
        return CreateModelFromData(new Contracts.Options());
      }

      public OptionsModel CreateModelFromData(Contracts.Options data)
      {
        var outlookAccountPasswordProvider = MockRepository.GenerateStub<IOutlookAccountPasswordProvider>();
        return new OptionsModel(
            MockRepository.GenerateStub<ISettingsFaultFinder>(),
            _optionTasksStub,
            outlookAccountPasswordProvider,
            data,
            new GeneralOptions(),
            this,
            false,
            new OptionModelSessionData(new Dictionary<string, OutlookCategory>()),
            new ServerSettingsDetector(outlookAccountPasswordProvider));
      }

      public IOptionsViewModel CreateViewModel(OptionsModel model)
      {
        return new GenericOptionsViewModel(
          MockRepository.GenerateStub<IOptionsViewModelParent>(),
          MockRepository.GenerateStub<IOptionsSection>(),
          _optionTasksStub,
          model,
          new string[0],
          MockRepository.GenerateStub<IViewOptions>());
      }

      public IOptionsViewModel CreateTemplateViewModel()
      {
        throw new NotImplementedException();
      }

      public ProfileModelOptions ModelOptions { get; } = new ProfileModelOptions(true, true, true, true, "DAV Url", true, true, true);

      public string ImageUrl { get; } = string.Empty;
      public Contracts.Options CreateOptions()
      {
        return new Contracts.Options();
      }

      public EventMappingConfiguration CreateEventMappingConfiguration()
      {
        return new EventMappingConfiguration();
      }

      public ContactMappingConfiguration CreateContactMappingConfiguration()
      {
        return new ContactMappingConfiguration();
      }

      public TaskMappingConfiguration CreateTaskMappingConfiguration()
      {
        return new TaskMappingConfiguration();
      }

      public IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
      {
        return this;
      }
    }

  }
}
