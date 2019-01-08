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
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Windows;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Scheduling;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.Models
{
  public class OptionsModel : ModelBase
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly bool _isGoogle;
    private bool _isActive;
    private string _name;

    private bool _enableChangeTriggeredSynchronization;
    private OutlookFolderDescriptor _selectedFolderOrNull;

    private string _calenderUrl;
    private string _emailAddress;
    private SecureString _password;
    private bool _useAccountPassword;
    private string _userName;
    private bool _useGoogleNativeApi;
    
    private ConflictResolution _conflictResolution;
    private int _synchronizationIntervalInMinutes;
    private SynchronizationMode _synchronizationMode;
    private int _chunkSize;
    private bool _isChunkedSynchronizationEnabled;

    private int _daysToSynchronizeInTheFuture;
    private int _daysToSynchronizeInThePast;
    private bool _useSynchronizationTimeRange;
    private bool _useWebDavCollectionSync;

    private bool _closeConnectionAfterEachRequest;
    private bool _preemptiveAuthentication;
    private SecureString _proxyPassword;
    private string _proxyUrl;
    private bool _proxyUseDefault;
    private bool _proxyUseManual;
    private string _proxyUserName;
    private bool _forceBasicAuthentication;
    private string _profileTypeOrNull;

    private MappingConfigurationModel _mappingConfigurationModelOrNull;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly ISettingsFaultFinder _faultFinder;
    private readonly IOptionTasks _optionTasks;
    private readonly GeneralOptions _generalOptions;
    private readonly OptionModelSessionData _sessionData;
    private readonly MappingConfigurationModelFactory _mappingConfigurationModelFactory;
    private readonly IServerSettingsDetector _serverSettingsDetector;

    public static OptionsModel DesignInstance => new OptionsModel(NullSettingsFaultFinder.Instance, NullOptionTasks.Instance, NullOutlookAccountPasswordProvider.Instance, new Contracts.Options(), new GeneralOptions(), DesignProfileModelFactory.Instance, false, new OptionModelSessionData(new  Dictionary<string, OutlookCategory>()), new NullServerSettingsDetector());
   
    public OptionsModel(
      ISettingsFaultFinder faultFinder, 
      IOptionTasks optionTasks, 
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      Contracts.Options data,
      GeneralOptions generalOptions, 
      IProfileModelFactory profileModelFactory,
      bool isGoogle,
      OptionModelSessionData sessionData, 
      IServerSettingsDetector serverSettingsDetector)
    {
      if (data == null) throw new ArgumentNullException(nameof(data));
      if (serverSettingsDetector == null) throw new ArgumentNullException(nameof(serverSettingsDetector));

      _mappingConfigurationModelFactory = new MappingConfigurationModelFactory(sessionData);
      _faultFinder = faultFinder ?? throw new ArgumentNullException(nameof(faultFinder));
      _optionTasks = optionTasks ?? throw new ArgumentNullException(nameof(optionTasks));
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider ?? throw new ArgumentNullException(nameof(outlookAccountPasswordProvider));
      _generalOptions = generalOptions ?? throw new ArgumentNullException(nameof(generalOptions));
      ModelFactory = profileModelFactory ?? throw new ArgumentNullException(nameof(profileModelFactory));
      _sessionData = sessionData ?? throw new ArgumentNullException(nameof(sessionData));

      Id = data.Id;

      _isGoogle = isGoogle;
      _serverSettingsDetector = serverSettingsDetector;

      InitializeData(data);
    }
 

    public IProfileModelFactory ModelFactory { get; }

    public Guid Id { get; }
    public MappingConfigurationModel MappingConfigurationModelOrNull
    {
      get { return _mappingConfigurationModelOrNull; }
      set
      {
        CheckedPropertyChange(ref _mappingConfigurationModelOrNull, value);
      }
    }

    public bool IsActive
    {
      get { return _isActive; }
      set
      {
        CheckedPropertyChange(ref _isActive, value);
      }
    }

    public string Name
    {
      get { return _name; }
      set
      {
        CheckedPropertyChange(ref _name, value);
      }
    }
    
    public bool EnableChangeTriggeredSynchronization
    {
      get { return _enableChangeTriggeredSynchronization; }
      set
      {
        CheckedPropertyChange (ref _enableChangeTriggeredSynchronization, value);
      }
    }

    public OutlookFolderDescriptor SelectedFolderOrNull
    {
      get { return _selectedFolderOrNull; }
      private set
      {
        if (CheckedPropertyChange(ref _selectedFolderOrNull, value))
        {
          OnPropertyChanged(nameof(UseGoogleNativeApiAvailable));
        }
      }
    }
    
    public string CalenderUrl
    {
      get { return _calenderUrl; }
      set
      {
        CheckedPropertyChange(ref _calenderUrl, value);
      }
    }

    public string UserName
    {
      get { return _userName; }
      set
      {
        CheckedPropertyChange(ref _userName, value);
      }
    }
    
    public SecureString Password
    {
      get { return _password; }
      set
      {
        CheckedPropertyChange(ref _password, value);
      }
    }

    public string EmailAddress
    {
      get { return _emailAddress; }
      set
      {
        CheckedPropertyChange(ref _emailAddress, value);
      }
    }

    public bool UseAccountPassword
    {
      get { return _useAccountPassword; }
      set
      {
        CheckedPropertyChange(ref _useAccountPassword, value);
      }
    }


    public SynchronizationMode SynchronizationMode
    {
      get { return _synchronizationMode; }
      set
      {
        CheckedPropertyChange(ref _synchronizationMode, value);
      }
    }

    public ConflictResolution Resolution
    {
      get { return _conflictResolution; }
      set
      {
        CheckedPropertyChange(ref _conflictResolution, value);
      }
    }

    public int SynchronizationIntervalInMinutes
    {
      get { return _synchronizationIntervalInMinutes; }
      set
      {
        CheckedPropertyChange(ref _synchronizationIntervalInMinutes, value);
      }
    }

    public int ChunkSize
    {
      get { return _chunkSize; }
      set { CheckedPropertyChange(ref _chunkSize, value); }
    }

    public bool IsChunkedSynchronizationEnabled
    {
      get { return _isChunkedSynchronizationEnabled; }
      set { CheckedPropertyChange(ref _isChunkedSynchronizationEnabled, value); }
    }

    public bool UseSynchronizationTimeRange
    {
      get { return _useSynchronizationTimeRange; }
      set
      {
        if (CheckedPropertyChange(ref _useSynchronizationTimeRange, value))
        {
          if (value)
            UseWebDavCollectionSync = false;
        }
      }
    }

    public bool UseWebDavCollectionSync
    {
      get { return _useWebDavCollectionSync; }
      set
      {
        if (CheckedPropertyChange(ref _useWebDavCollectionSync, value))
        {
          if (value)
          {
            UseSynchronizationTimeRange = false;
            UseGoogleNativeApi = false;
          }
        }
      }
    }

    public int DaysToSynchronizeInThePast
    {
      get { return _daysToSynchronizeInThePast; }
      set
      {
        CheckedPropertyChange(ref _daysToSynchronizeInThePast, value);
      }
    }

    public int DaysToSynchronizeInTheFuture
    {
      get { return _daysToSynchronizeInTheFuture; }
      set
      {
        CheckedPropertyChange(ref _daysToSynchronizeInTheFuture, value);
      }
    }

    public bool CloseConnectionAfterEachRequest
    {
      get { return _closeConnectionAfterEachRequest; }
      set
      {
        CheckedPropertyChange(ref _closeConnectionAfterEachRequest, value);
      }
    }

    public bool PreemptiveAuthentication
    {
      get { return _preemptiveAuthentication; }
      set
      {
        CheckedPropertyChange(ref _preemptiveAuthentication, value);
      }
    }

    public bool ProxyUseDefault
    {
      get { return _proxyUseDefault; }
      set
      {
        if (value)
          ProxyUseManual = false;

        CheckedPropertyChange(ref _proxyUseDefault, value);
      }
    }

    public bool ProxyUseManual
    {
      get { return _proxyUseManual; }
      set
      {
        if (value)
          ProxyUseDefault = false;

        CheckedPropertyChange(ref _proxyUseManual, value);
      }
    }

    public string ProxyUrl
    {
      get { return _proxyUrl; }
      set
      {
        CheckedPropertyChange(ref _proxyUrl, value);
      }
    }

    public string ProxyUserName
    {
      get { return _proxyUserName; }
      set
      {
        CheckedPropertyChange(ref _proxyUserName, value);
      }
    }

    public SecureString ProxyPassword
    {
      get { return _proxyPassword; }
      set
      {
        CheckedPropertyChange(ref _proxyPassword, value);
      }
    }

    public bool ForceBasicAuthentication
    {
      get { return _forceBasicAuthentication; }
      set
      {
        CheckedPropertyChange(ref _forceBasicAuthentication, value);
      }
    }

    public bool UseGoogleNativeApi
    {
      get { return _useGoogleNativeApi; }
      set
      {
        if (value)
          UseWebDavCollectionSync = false;

        CheckedPropertyChange(ref _useGoogleNativeApi, value);
      }
    }

    public bool UseGoogleNativeApiAvailable => _isGoogle && SelectedFolderOrNull?.DefaultItemType == OlItemType.olContactItem;

    
    private ServerAdapterType ServerAdapterType
    {
      get
      {
        if (_isGoogle)
        {
          switch (SelectedFolderOrNull?.DefaultItemType)
          {
            case OlItemType.olTaskItem:
              return ServerAdapterType.GoogleTaskApi;
            case OlItemType.olContactItem:
              return UseGoogleNativeApi ? ServerAdapterType.GoogleContactApi : ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth;
            default:
              return ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth;
          }
        }
        else
        {
          return ServerAdapterType.WebDavHttpClientBased;
        }
      }
    }

    public string FolderAccountName { get; private set; }

    /// <remarks>
    /// InitializeData has to set fields instead of properties, since properties can encapsulate business logic and can interfer with each other!
    /// </remarks>
    private void InitializeData(Contracts.Options data)
    {
      _name = data.Name;
      _isActive = !data.Inactive;

      _enableChangeTriggeredSynchronization = data.EnableChangeTriggeredSynchronization;
    
      InitializeFolder(data);

      FolderAccountName = data.OutlookFolderAccountName;

      _calenderUrl = data.CalenderUrl;
      _userName = data.UserName;
      _password = data.Password;
      _emailAddress = data.EmailAddress;
      _useAccountPassword = data.UseAccountPassword;

      _useGoogleNativeApi = data.ServerAdapterType == ServerAdapterType.GoogleContactApi || data.ServerAdapterType == ServerAdapterType.GoogleTaskApi;

      _synchronizationMode = data.SynchronizationMode;
      _conflictResolution = data.ConflictResolution;
      _synchronizationIntervalInMinutes = data.SynchronizationIntervalInMinutes;
      _isChunkedSynchronizationEnabled = data.IsChunkedSynchronizationEnabled;
      _chunkSize = data.ChunkSize;

      _useSynchronizationTimeRange = !data.IgnoreSynchronizationTimeRange;
      _daysToSynchronizeInThePast = data.DaysToSynchronizeInThePast;
      _daysToSynchronizeInTheFuture = data.DaysToSynchronizeInTheFuture;
      _useWebDavCollectionSync = data.UseWebDavCollectionSync;
      _profileTypeOrNull = data.ProfileTypeOrNull;

      var proxyOptions = data.ProxyOptions ?? new ProxyOptions();

      _closeConnectionAfterEachRequest = data.CloseAfterEachRequest;
      _preemptiveAuthentication = data.PreemptiveAuthentication;
      _forceBasicAuthentication = data.ForceBasicAuthentication;
      _proxyUseDefault = proxyOptions.ProxyUseDefault;
      _proxyUseManual = proxyOptions.ProxyUseManual;
      _proxyUrl = proxyOptions.ProxyUrl;
      _proxyUserName = proxyOptions.ProxyUserName;
      _proxyPassword = proxyOptions.ProxyPassword;

      MappingConfigurationModelOrNull = data.MappingConfiguration?.Accept(_mappingConfigurationModelFactory);
      CoerceMappingConfiguration();
    }

    private void InitializeFolder(Contracts.Options data)
    {
      if (!string.IsNullOrEmpty(data.OutlookFolderEntryId) && !string.IsNullOrEmpty(data.OutlookFolderStoreId))
      {
        try
        {
           _selectedFolderOrNull = _optionTasks.GetFolderFromId(data.OutlookFolderEntryId, data.OutlookFolderStoreId);
        }
        catch (System.Exception x)
        {
          s_logger.Error(null, x);
          _selectedFolderOrNull = null;
        }
      }
      else
      {
        _selectedFolderOrNull = null;
      }
    }

    public Contracts.Options CreateData()
    {
      return new Contracts.Options
      {
        Id = Id,
        Name = Name,
        Inactive = !IsActive,
        EnableChangeTriggeredSynchronization = _enableChangeTriggeredSynchronization,
        OutlookFolderEntryId = _selectedFolderOrNull?.EntryId,
        OutlookFolderStoreId = _selectedFolderOrNull?.StoreId,
        OutlookFolderAccountName = FolderAccountName,
        CalenderUrl = _calenderUrl,
        UserName = _userName,
        Password = _password,
        EmailAddress = _emailAddress,
        UseAccountPassword = _useAccountPassword,
        ServerAdapterType = ServerAdapterType,
        SynchronizationMode = _synchronizationMode,
        ConflictResolution = _conflictResolution,
        SynchronizationIntervalInMinutes = _synchronizationIntervalInMinutes,
        IsChunkedSynchronizationEnabled = IsChunkedSynchronizationEnabled,
        ChunkSize = ChunkSize,
        IgnoreSynchronizationTimeRange = !_useSynchronizationTimeRange,
        UseWebDavCollectionSync = _useWebDavCollectionSync,
        DaysToSynchronizeInThePast = _daysToSynchronizeInThePast,
        DaysToSynchronizeInTheFuture = _daysToSynchronizeInTheFuture,
        CloseAfterEachRequest = _closeConnectionAfterEachRequest,
        PreemptiveAuthentication = _preemptiveAuthentication,
        ForceBasicAuthentication = _forceBasicAuthentication,
        ProxyOptions = CreateProxyOptions(),
        MappingConfiguration = MappingConfigurationModelOrNull?.GetData(),
        ProfileTypeOrNull = _profileTypeOrNull
      };
    }

    public void AddOneTimeTasks(Action<OneTimeChangeCategoryTask> add)
    {
      MappingConfigurationModelOrNull?.AddOneTimeTasks(add);
    }

    public OptionsModel Clone()
    {
      var data = CreateData();
      data.Id = Guid.NewGuid();
      return new OptionsModel(_faultFinder, _optionTasks, _outlookAccountPasswordProvider, data , _generalOptions, ModelFactory, _isGoogle, _sessionData, _serverSettingsDetector);
    }

    public ProxyOptions CreateProxyOptions()
    {
      return new ProxyOptions
      {
        ProxyUseDefault = _proxyUseDefault,
        ProxyUseManual = _proxyUseManual,
        ProxyUrl = _proxyUrl,
        ProxyUserName = _proxyUserName,
        ProxyPassword = _proxyPassword
      };
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (_selectedFolderOrNull == null)
      {
        errorMessageBuilder.AppendLine (Strings.Get($"- There is no Outlook folder selected."));
        result = false;
      }

      if (_useWebDavCollectionSync && _selectedFolderOrNull?.DefaultItemType != OlItemType.olAppointmentItem && _selectedFolderOrNull?.DefaultItemType != OlItemType.olContactItem)
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- WebDav collection sync is currently just supported for appointments and contacts."));
        result = false;
      }

      if (_isGoogle)
      {
        var serverAdapterType = ServerAdapterType;
        if (serverAdapterType != ServerAdapterType.GoogleTaskApi && serverAdapterType != ServerAdapterType.GoogleContactApi)
          result &= OptionTasks.ValidateWebDavUrl(CalenderUrl, errorMessageBuilder, true);

        result &= OptionTasks.ValidateGoogleEmailAddress(errorMessageBuilder, EmailAddress);
      }
      else
      {
        result &= OptionTasks.ValidateWebDavUrl(CalenderUrl, errorMessageBuilder, true);
        result &= OptionTasks.ValidateEmailAddress(errorMessageBuilder, EmailAddress);
      }

      if (IsChunkedSynchronizationEnabled && ChunkSize < 1)
      {
        result = false;
        errorMessageBuilder.AppendLine(Strings.Get($"- The chunk size has to be 1 or greater."));
      }

      if (MappingConfigurationModelOrNull != null)
        result &= MappingConfigurationModelOrNull.Validate(errorMessageBuilder);

      return result;
    }

    public bool SetFolder(OutlookFolderDescriptor folderDescriptor)
    {
      if (folderDescriptor.DefaultItemType != OlItemType.olAppointmentItem && folderDescriptor.DefaultItemType != OlItemType.olTaskItem && folderDescriptor.DefaultItemType != OlItemType.olContactItem)
        return false;

      SelectedFolderOrNull = folderDescriptor;

      FolderAccountName = _selectedFolderOrNull != null
         ? _optionTasks.GetFolderAccountNameOrNull(_selectedFolderOrNull.StoreId)
         : null;

      _faultFinder.FixTimeRangeUsage(this, folderDescriptor.DefaultItemType);

      CoerceMappingConfiguration();

      return true;
    }
    
    private void CoerceMappingConfiguration()
    {
      MappingConfigurationModelOrNull = CoerceMappingConfiguration(MappingConfigurationModelOrNull, SelectedFolderOrNull?.DefaultItemType);
    }

    private MappingConfigurationModel CoerceMappingConfiguration(
      MappingConfigurationModel currentMappingConfiguration,
      OlItemType? outlookFolderType)
    {
      switch (outlookFolderType)
      {
        case OlItemType.olAppointmentItem:
          return currentMappingConfiguration as EventMappingConfigurationModel ?? new EventMappingConfigurationModel(ModelFactory.ProfileType.CreateEventMappingConfiguration(), _sessionData);
        case OlItemType.olContactItem:
          return ModelFactory.ModelOptions.IsContactMappingConfigurationEnabled
            ? currentMappingConfiguration as ContactMappingConfigurationModel ?? new ContactMappingConfigurationModel(ModelFactory.ProfileType.CreateContactMappingConfiguration())
            : null;
        case OlItemType.olTaskItem:
          return ModelFactory.ModelOptions.IsTaskMappingConfigurationEnabled
            ? currentMappingConfiguration as TaskMappingConfigurationModel ?? new TaskMappingConfigurationModel(ModelFactory.ProfileType.CreateTaskMappingConfiguration())
            : null;
        default:
          return currentMappingConfiguration;
      }
    }
   

    public void AutoFillAccountSettings()
    {
      try
      {
        _serverSettingsDetector.AutoFillServerSettings(this);
      }
      catch (System.Exception x)
      {
        s_logger.Error("Exception while getting account settings.", x);
        string message = null;
        for (System.Exception ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show(message, Strings.Get($"Account settings"));
      }
    }

    public IWebDavClient CreateWebDavClient(Uri url = null)
    {
      return SynchronizerFactory.CreateWebDavClient(
          UserName,
          UseAccountPassword ? _outlookAccountPasswordProvider.GetPassword(FolderAccountName) : Password,
          url != null ? url.ToString() : CalenderUrl,
          _generalOptions.CalDavConnectTimeout,
          ServerAdapterType,
          CloseConnectionAfterEachRequest,
          PreemptiveAuthentication,
          ForceBasicAuthentication,
          CreateProxyOptions(),
          _generalOptions.EnableClientCertificate,
          _generalOptions.AcceptInvalidCharsInServerResponse);
    }

    public IWebProxy GetProxyIfConfigured()
    {
      return SynchronizerFactory.CreateProxy(CreateProxyOptions());
    }

    public ICalDavDataAccess CreateCalDavDataAccess()
    {
      var calendarUrl = new Uri(CalenderUrl);
      return new CalDavDataAccess(calendarUrl, CreateWebDavClient(calendarUrl));
    }

    private class MappingConfigurationModelFactory : IMappingConfigurationBaseVisitor<MappingConfigurationModel>
    {
      private readonly OptionModelSessionData _sessionData;

      public MappingConfigurationModelFactory(OptionModelSessionData sessionData)
      {
        _sessionData = sessionData ?? throw new ArgumentNullException(nameof(sessionData));
      }

      public MappingConfigurationModel Visit(ContactMappingConfiguration configuration)
      {
        return new ContactMappingConfigurationModel(configuration);
      }

      public MappingConfigurationModel Visit(EventMappingConfiguration configuration)
      {
        return new EventMappingConfigurationModel(configuration, _sessionData);
      }

      public MappingConfigurationModel Visit(TaskMappingConfiguration configuration)
      {
        return new TaskMappingConfigurationModel(configuration);
      }
    }
  }
}