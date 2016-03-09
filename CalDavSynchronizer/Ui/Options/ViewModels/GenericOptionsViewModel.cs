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
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class GenericOptionsViewModel : OptionsViewModelBase, ICurrentOptions, ISyncSettingsControl
  {
    private readonly ObservableCollection<IOptionsViewModel> _subOptions = new ObservableCollection<IOptionsViewModel>();
    private readonly NetworkSettingsViewModel _networkSettingsViewModel;
    private readonly OutlookFolderViewModel _outlookFolderViewModel;
    private readonly IServerSettingsViewModel _serverSettingsViewModel;
    private readonly SyncSettingsViewModel _syncSettingsViewModel;
    private IOptionsViewModel _mappingConfigurationViewModel;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly IMappingConfigurationViewModelFactory _mappingConfigurationViewModelFactory;

    public GenericOptionsViewModel (
        NameSpace session,
        IOptionsViewModelParent parent,
        bool fixInvalidSettings,
        IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
        Func<ISettingsFaultFinder, ICurrentOptions, IServerSettingsViewModel> serverSettingsViewModelFactory,
        Func<ICurrentOptions, IMappingConfigurationViewModelFactory> mappingConfigurationViewModelFactoryFactory)
        : base (parent)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));
      if (mappingConfigurationViewModelFactoryFactory == null)
        throw new ArgumentNullException (nameof (mappingConfigurationViewModelFactoryFactory));

      _syncSettingsViewModel = new SyncSettingsViewModel();
      _networkSettingsViewModel = new NetworkSettingsViewModel();

      var faultFinder = fixInvalidSettings ? new SettingsFaultFinder (this) : NullSettingsFaultFinder.Instance;
      _serverSettingsViewModel = serverSettingsViewModelFactory (faultFinder, this);
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
      _mappingConfigurationViewModelFactory = mappingConfigurationViewModelFactoryFactory(this);
      _outlookFolderViewModel = new OutlookFolderViewModel (session, faultFinder);
      _outlookFolderViewModel.PropertyChanged += OutlookFolderViewModel_PropertyChanged;
      _serverSettingsViewModel.PropertyChanged += ServerSettingsViewModel_PropertyChanged;
    }

    /// <remarks>
    /// Just for creating the DesingInstance
    /// </remarks>
    public GenericOptionsViewModel (IOptionsViewModelParent parent, NetworkSettingsViewModel networkSettingsViewModel, OutlookFolderViewModel outlookFolderViewModel, IServerSettingsViewModel serverSettingsViewModel, SyncSettingsViewModel syncSettingsViewModel, IOptionsViewModel mappingConfigurationViewModel)
        : base (parent)
    {
      _networkSettingsViewModel = networkSettingsViewModel;
      _outlookFolderViewModel = outlookFolderViewModel;
      _serverSettingsViewModel = serverSettingsViewModel;
      _syncSettingsViewModel = syncSettingsViewModel;
      MappingConfigurationViewModel = mappingConfigurationViewModel;
    }

    private IOptionsViewModel MappingConfigurationViewModel
    {
      get { return _mappingConfigurationViewModel; }
      set
      {
        if (!ReferenceEquals (value, _mappingConfigurationViewModel))
        {
          _subOptions.Remove (_mappingConfigurationViewModel);
          if (value != null)
            _subOptions.Add (value);
          _mappingConfigurationViewModel = value;
        }
      }
    }

    protected override void SetOptionsOverride (CalDavSynchronizer.Contracts.Options options)
    {
      MappingConfigurationViewModel = options.MappingConfiguration?.CreateConfigurationViewModel (_mappingConfigurationViewModelFactory);

      CoerceMappingConfiguration();

      MappingConfigurationViewModel?.SetOptions (options);
    }

    private void ServerSettingsViewModel_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof (IServerSettingsViewModel.ServerAdapterType))
        CoerceMappingConfiguration();
    }

    private void OutlookFolderViewModel_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof (OutlookFolderViewModel.OutlookFolderType))
      {
        CoerceMappingConfiguration();
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof (OutlookFolderType));
      }
    }

    protected override IEnumerable<IOptionsViewModel> CreateSubOptions ()
    {
      _subOptions.Add (_networkSettingsViewModel);
      return _subOptions;
    }

    protected override IEnumerable<IOptionsSection> CreateSections ()
    {
      return new IOptionsSection[] { _outlookFolderViewModel, _serverSettingsViewModel, _syncSettingsViewModel };
    }

    private void CoerceMappingConfiguration ()
    {
      MappingConfigurationViewModel = OptionTasks.CoerceMappingConfiguration (
          MappingConfigurationViewModel,
          _outlookFolderViewModel.OutlookFolderType,
          _serverSettingsViewModel.ServerAdapterType == ServerAdapterType.GoogleTaskApi,
          _mappingConfigurationViewModelFactory);
    }


    public SynchronizationMode SynchronizationMode
    {
      get { return _syncSettingsViewModel.SynchronizationMode; }
      set { _syncSettingsViewModel.SynchronizationMode = value; }
    }

    public IList<Item<SynchronizationMode>> AvailableSynchronizationModes => _syncSettingsViewModel.AvailableSynchronizationModes;

    public bool UseSynchronizationTimeRange
    {
      get { return _syncSettingsViewModel.UseSynchronizationTimeRange; }
      set { _syncSettingsViewModel.UseSynchronizationTimeRange = value; }
    }

    public string SynchronizationModeDisplayName => _syncSettingsViewModel.SelectedSynchronizationModeDisplayName;

    public string ServerUrl
    {
      get { return _serverSettingsViewModel.CalenderUrl; }
      set { _serverSettingsViewModel.CalenderUrl = value; }
    }


    public IWebDavClient CreateWebDavClient ()
    {
      return SynchronizerFactory.CreateWebDavClient (
          _serverSettingsViewModel.UserName,
          _serverSettingsViewModel.UseAccountPassword ? _outlookAccountPasswordProvider.GetPassword (_outlookFolderViewModel.FolderAccountName) : _serverSettingsViewModel.Password,
          _serverSettingsViewModel.CalenderUrl,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
          _serverSettingsViewModel.ServerAdapterType,
          _networkSettingsViewModel.CloseConnectionAfterEachRequest,
          _networkSettingsViewModel.PreemptiveAuthentication,
          _networkSettingsViewModel.ForceBasicAuthentication,
          _networkSettingsViewModel.CreateProxyOptions());
    }

    public IWebProxy GetProxyIfConfigured ()
    {
      return SynchronizerFactory.CreateProxy (_networkSettingsViewModel.CreateProxyOptions());
    }

    public ICalDavDataAccess CreateCalDavDataAccess ()
    {
      return new CalDavDataAccess (new Uri (_serverSettingsViewModel.CalenderUrl), CreateWebDavClient ());
    }

    public OlItemType? OutlookFolderType => _outlookFolderViewModel.OutlookFolderType;

    public string EmailAddress => _serverSettingsViewModel.EmailAddress;

    public ServerAdapterType ServerAdapterType
    {
      get { return _serverSettingsViewModel.ServerAdapterType; }
      set { _serverSettingsViewModel.ServerAdapterType = value; }
    }

    public static GenericOptionsViewModel DesignInstance => new GenericOptionsViewModel (
        new DesignOptionsViewModelParent(),
        NetworkSettingsViewModel.DesignInstance,
        OutlookFolderViewModel.DesignInstance,
        ServerSettingsViewModel.DesignInstance,
        SyncSettingsViewModel.DesignInstance,
        EventMappingConfigurationViewModel.DesignInstance)
                                                            {
                                                                IsActive = true,
                                                                Name = "Test Profile",
                                                            };
  }
}