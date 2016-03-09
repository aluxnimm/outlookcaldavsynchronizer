using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class NetworkSettingsViewModel : ViewModelBase, IOptionsViewModel
  {
    private readonly ObservableCollection<IOptionsViewModel> _subOptions = new ObservableCollection<IOptionsViewModel>();
    private bool _closeConnectionAfterEachRequest;
    private bool _preemptiveAuthentication;
    private string _proxyPassword;
    private string _proxyUrl;
    private bool _proxyUseDefault;
    private bool _proxyUseManual;
    private string _proxyUserName;
    private bool _forceBasicAuthentication;
    private bool _isSelected;

    public bool CloseConnectionAfterEachRequest
    {
      get { return _closeConnectionAfterEachRequest; }
      set
      {
        _closeConnectionAfterEachRequest = value;
        OnPropertyChanged();
      }
    }

    public bool PreemptiveAuthentication
    {
      get { return _preemptiveAuthentication; }
      set
      {
        _preemptiveAuthentication = value;
        OnPropertyChanged();
      }
    }

    public bool ProxyUseDefault
    {
      get { return _proxyUseDefault; }
      set
      {
        _proxyUseDefault = value;
        OnPropertyChanged();
      }
    }

    public bool ProxyUseManual
    {
      get { return _proxyUseManual; }
      set
      {
        _proxyUseManual = value;
        OnPropertyChanged();
      }
    }

    public string ProxyUrl
    {
      get { return _proxyUrl; }
      set
      {
        _proxyUrl = value;
        OnPropertyChanged();
      }
    }

    public string ProxyUserName
    {
      get { return _proxyUserName; }
      set
      {
        _proxyUserName = value;
        OnPropertyChanged();
      }
    }

    public string ProxyPassword
    {
      get { return _proxyPassword; }
      set
      {
        _proxyPassword = value;
        OnPropertyChanged();
      }
    }

    public static NetworkSettingsViewModel DesignInstance => new NetworkSettingsViewModel
                                                             {
                                                                 CloseConnectionAfterEachRequest = true,
                                                                 PreemptiveAuthentication = true,
                                                                 ProxyPassword = "proxypassword",
                                                                 ProxyUrl = "proxyurl",
                                                                 ProxyUseDefault = true,
                                                                 ProxyUseManual = true,
                                                                 ProxyUserName = "proxyusername"
                                                             };


    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      var proxyOptions = options.ProxyOptions ?? new ProxyOptions();

      CloseConnectionAfterEachRequest = options.CloseAfterEachRequest;
      PreemptiveAuthentication = options.PreemptiveAuthentication;
      ProxyUseDefault = proxyOptions.ProxyUseDefault;
      ProxyUseManual = proxyOptions.ProxyUseManual;
      ProxyUrl = proxyOptions.ProxyUrl;
      ProxyUserName = proxyOptions.ProxyUserName;
      ProxyPassword = proxyOptions.ProxyPassword;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.CloseAfterEachRequest = _closeConnectionAfterEachRequest;
      options.PreemptiveAuthentication = _preemptiveAuthentication;
      options.ForceBasicAuthentication = _forceBasicAuthentication;
      options.ProxyOptions = CreateProxyOptions();
    }

    public ProxyOptions CreateProxyOptions ()
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

    public string Name => "Network settings";

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }

    public IEnumerable<IOptionsViewModel> SubOptions => _subOptions;

    public bool ForceBasicAuthentication
    {
      get { return _forceBasicAuthentication; }
      set
      {
        _forceBasicAuthentication = value;
        OnPropertyChanged();
      }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        _isSelected = value;
        OnPropertyChanged();
      }
    }
  }
}