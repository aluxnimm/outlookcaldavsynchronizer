using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.OAuth.Daimler;
using CalDavSynchronizer.OAuth.Daimler.Models;
using CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.Helpers;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.ViewModels
{
    public class DaimlerServerSettingsViewModel : ServerSettingsViewModel
    {
        private readonly GeneralOptions _generalOptions;
        private readonly OptionsModel _model;
        private readonly IOptionTasks _optionTasks;
        private readonly ProfileDataProvider _profileDataProvider;

        private readonly DelegateCommandWithoutCanExecuteDelegation _loginCommandDelegate;
        private readonly DelegateCommandWithoutCanExecuteDelegation _testConnectionCommand;
        private readonly DelegateCommandWithoutCanExecuteDelegation _changeResourceCommand;

        private bool _isLoading = false;
        private bool _isDiscovering = false;

        public ICommand LoginCommand => _loginCommandDelegate;
        public ICommand TestOrDiscoverCommand => _testConnectionCommand;
        public ICommand ChangeResourceCommand => _changeResourceCommand;

        private DaimlerOptions _daimlerConfig;
        public DaimlerOptions DaimlerConfig
        {
            get => _daimlerConfig;
            set
            {
                _daimlerConfig = value;
                ConfigLoaded = value != null;

                if (!_isLoading) _profileDataProvider.SaveConfig(value);

                Environments = _daimlerConfig?.Environments ?? Array.Empty<DaimlerEnvironment>();
            }
        }

        private DaimlerEnvironment[] _environments = Array.Empty<DaimlerEnvironment>();
        public DaimlerEnvironment[] Environments
        {
            get => _environments;
            set
            {
                _environments = value;
                OnPropertyChanged(nameof(Environments));

                if (SelectedEnvironment == null && _environments.Length > 0)
                    SelectedEnvironment = _environments[0];
            }
        }

        private DaimlerEnvironment _selectedEnvironment;
        public DaimlerEnvironment SelectedEnvironment
        {
            get => _selectedEnvironment;
            set
            {
                _selectedEnvironment = value;
                OnPropertyChanged(nameof(SelectedEnvironment));

                if (_isLoading) return;

                CalenderUrl = value.Url.EnsureEndsWith("/");
                Token = _profileDataProvider.LoadToken(value.Name).Result;
                SaveProfileOptions();
            }
        }

        private bool _configLoaded = false;
        public bool ConfigLoaded
        {
            get => _configLoaded;
            private set
            {
                _configLoaded = value;
                _loginCommandDelegate.SetCanExecute(true);
                OnPropertyChanged(nameof(ConfigLoaded));
            }
        }

        private bool _showLogin = true;
        public bool ShowLogin
        {
            get => _showLogin;
            private set
            {
                _showLogin = value;
                OnPropertyChanged(nameof(ShowLogin));
            }

        }

        private bool _isAuthProgress = false;
        public bool IsAuthProgress
        {
            get => _isAuthProgress;
            set
            {
                _isAuthProgress = value;
                OnPropertyChanged(nameof(IsAuthProgress));
            }
        }

        public bool IsAuthenticated => !Token?.IsExpired ?? false;

        private TokenData _token;
        public TokenData Token
        {
            get => _token;
            private set
            {
                _token = value;
                ShowLogin = !IsAuthenticated;
                if (_isLoading) return;
                _profileDataProvider.SaveToken(_token, SelectedEnvironment.Name);
            }
        }

        public OutlookFolderDescriptor Folder => _model.SelectedFolderOrNull;

        public DaimlerServerSettingsViewModel(
            OptionsModel model,
            GeneralOptions generalOptions,
            IOptionTasks optionTasks,
            IViewOptions viewOptions)
            : base(model, optionTasks, viewOptions)
        {
            _model = model;
            _generalOptions = generalOptions;
            _optionTasks = optionTasks;

            _loginCommandDelegate = new DelegateCommandWithoutCanExecuteDelegation(async _ =>
            {
                ComponentContainer.EnsureSynchronizationContext();
                await Login();
            });
            _loginCommandDelegate.SetCanExecute(false);

            _testConnectionCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
            {
                ComponentContainer.EnsureSynchronizationContext();
                _isDiscovering = true;
                TestConnectionAsync();
            });
            _testConnectionCommand.SetCanExecute(false);

            _changeResourceCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
            {
                _changeResourceCommand.SetCanExecute(false);
                _isDiscovering = true;
                ComponentContainer.EnsureSynchronizationContext();
                CalenderUrl = SelectedEnvironment.Url;
                _testConnectionCommand.SetCanExecute(true);
                _testConnectionCommand.Execute(null);
            });
            _changeResourceCommand.SetCanExecute(false);

            RegisterPropertyChangePropagation(_model, nameof(_model.SelectedFolderOrNull), nameof(Folder));

            PropertyChanged += DaimlerServerSettingsViewModel_PropertyChanged;
            _profileDataProvider = new ProfileDataProvider(profileId: model.Id, useRoamingFolder: generalOptions.StoreAppDataInRoamingFolder);

            LoadConfiguration();
        }

        private async void TestConnectionAsync()
        {
            _testConnectionCommand.SetCanExecute(false);
            try
            {
                var newUrl = await _optionTasks.TestWebDavConnection(_model);
                if (newUrl == SelectedEnvironment.Url) return;

                CalenderUrl = newUrl;
                SaveProfileOptions();
            }
            catch (Exception x)
            {
                string message = null;
                for (Exception ex = x; ex != null; ex = ex.InnerException)
                    message += ex.Message + Environment.NewLine;
                MessageBox.Show(message, OptionTasks.ConnectionTestCaption);
            }
            finally
            {
                _testConnectionCommand.SetCanExecute(true);
                _changeResourceCommand.SetCanExecute(true);
                _isDiscovering = false;
            }
        }

        private void LoadConfiguration()
        {
            _isLoading = true;

            var config = _profileDataProvider.LoadConfig();
            if (config == null)
            {
                _isLoading = false;
                return;
            }

            DaimlerConfig = config;
            var profile = _profileDataProvider.LoadProfileOptions();
            if (profile == null)
            {
                _isLoading = false;
                return;
            }

            SelectedEnvironment = config.Environments.First(env => env.Name == profile.SelectedEnvironment);
            CalenderUrl = profile.CalenderUrl;

            Token = _profileDataProvider.LoadToken(SelectedEnvironment.Name).Result;

            _isLoading = false;
            _changeResourceCommand.SetCanExecute(true);
            _testConnectionCommand.SetCanExecute(true);
        }

        private void DaimlerServerSettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SelectedEnvironment == null) return;

            switch (e.PropertyName)
            {
                case nameof(Folder):
                    UpdateCalenderUrl();
                    return;
                case nameof(CalenderUrl):
                    if (_isLoading || _isDiscovering) return;
                    SaveProfileOptions();
                    return;
            }
        }

        private async Task Login()
        {
            IsAuthProgress = true;
            _loginCommandDelegate.SetCanExecute(false);

            try
            {
                var authSvc = new AuthenticationService(SelectedEnvironment);
                Token = await authSvc.Authenticate();
                UserName = Token.Username;

                //UpdateCalenderUrl();
                _testConnectionCommand.SetCanExecute(true);
            }
            catch (Exception x)
            {
                string message = null;
                for (Exception ex = x; ex != null; ex = ex.InnerException)
                    message += ex.Message + Environment.NewLine;

                MessageBox.Show(
                        messageBoxText: "Unauthorized",
                        caption: "Error",
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Error);
            }
            finally
            {
                _loginCommandDelegate.SetCanExecute(true);
                IsAuthProgress = false;
            }
        }

        // private void UpdateCalenderUrl() => CalenderUrl = $"{SelectedEnvironment.Url}/caldav.php/{UserName}/{(Folder?.DefaultItemType != OlItemType.olContactItem ? "calendar" : "addresses")}";
        private void UpdateCalenderUrl()
        {
            if (SelectedEnvironment == null)
            {
                CalenderUrl = string.Empty;
                return;
            }

            CalenderUrl = SelectedEnvironment.Url.EnsureEndsWith("/");

            //if (string.IsNullOrWhiteSpace(UserName))
            //{
            //    CalenderUrl = SelectedEnvironment.Url.EnsureEndsWith("/");
            //    return;
            //}

            //CalenderUrl = $"{SelectedEnvironment.Url.EnsureEndsWith("/")}{UserName}/{(Folder?.DefaultItemType != OlItemType.olContactItem ? "calendar" : "addresses").EnsureEndsWith("/")}";
        }

        private void SaveProfileOptions() =>
            _profileDataProvider.SaveProfileOptions(
                new ProfileOptions
                {
                    ProfileId = _model.Id,
                    SelectedEnvironment = SelectedEnvironment.Name,
                    CalenderUrl = CalenderUrl
                });
    }
}
