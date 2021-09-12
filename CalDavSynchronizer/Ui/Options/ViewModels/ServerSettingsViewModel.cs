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
using System.Reflection;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Utilities;
using log4net;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
    public class ServerSettingsViewModel : ModelBase, IOptionsSection
    {
        private readonly OptionsModel _model;
        private static readonly ILog s_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOptionTasks _optionTasks;


        private readonly DelegateCommandWithoutCanExecuteDelegation _testConnectionCommand;
        private readonly DelegateCommandWithoutCanExecuteDelegation _getAccountSettingsCommand;
        private readonly DelegateCommandWithoutCanExecuteDelegation _createDavResourceCommand;

        public ServerSettingsViewModel(OptionsModel model, IOptionTasks optionTasks, IViewOptions viewOptions)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
            if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

            _model = model;
            _optionTasks = optionTasks;
            ViewOptions = viewOptions;

            _testConnectionCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
            {
                ComponentContainer.EnsureSynchronizationContext();
                TestConnectionAsync();
            });
            _getAccountSettingsCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
            {
                ComponentContainer.EnsureSynchronizationContext();
                _model.AutoFillAccountSettings();
            });
            _createDavResourceCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
            {
                ComponentContainer.EnsureSynchronizationContext();
                CreateDavResource();
            });

            RegisterPropertyChangePropagation(_model, nameof(_model.CalenderUrl), nameof(CalenderUrl));
            RegisterPropertyChangePropagation(_model, nameof(_model.UserName), nameof(UserName));
            RegisterPropertyChangePropagation(_model, nameof(_model.Password), nameof(Password));
            RegisterPropertyChangePropagation(_model, nameof(_model.EmailAddress), nameof(EmailAddress));
            RegisterPropertyChangePropagation(_model, nameof(_model.UseAccountPassword), nameof(UseAccountPassword));
            RegisterPropertyChangePropagation(_model, nameof(_model.UseWebDavCollectionSync), nameof(UseWebDavCollectionSync));
        }

        public ICommand TestConnectionCommand => _testConnectionCommand;
        public ICommand GetAccountSettingsCommand => _getAccountSettingsCommand;
        public ICommand CreateDavResourceCommand => _createDavResourceCommand;

        public string CalenderUrl
        {
            get { return _model.CalenderUrl; }
            set { _model.CalenderUrl = value; }
        }

        public string UserName
        {
            get { return _model.UserName; }
            set { _model.UserName = value; }
        }

        public SecureString Password
        {
            get { return _model.Password; }
            set { _model.Password = value; }
        }

        public string EmailAddress
        {
            get { return _model.EmailAddress; }
            set { _model.EmailAddress = value; }
        }

        public bool UseAccountPassword
        {
            get { return _model.UseAccountPassword; }
            set { _model.UseAccountPassword = value; }
        }

        public bool UseWebDavCollectionSync
        {
            get { return _model.UseWebDavCollectionSync; }
            set { _model.UseWebDavCollectionSync = value; }
        }

        public static ServerSettingsViewModel DesignInstance => new ServerSettingsViewModel(OptionsModel.DesignInstance, NullOptionTasks.Instance, OptionsCollectionViewModel.DesignViewOptions)
        {
            CalenderUrl = "http://calendar.url",
            EmailAddress = "bla@dot.com",
            Password = SecureStringUtility.ToSecureString("password"),
            UseAccountPassword = true,
            UserName = "username",
            UseWebDavCollectionSync = false
        };

        private async void TestConnectionAsync()
        {
            _testConnectionCommand.SetCanExecute(false);
            try
            {
                CalenderUrl = await _optionTasks.TestWebDavConnection(_model);
            }
            catch (Exception x)
            {
                s_logger.Error("Exception while testing the connection.", x);
                string message = null;
                for (Exception ex = x; ex != null; ex = ex.InnerException)
                    message += ex.Message + Environment.NewLine;
                MessageBox.Show(message, OptionTasks.ConnectionTestCaption);
            }
            finally
            {
                _testConnectionCommand.SetCanExecute(true);
            }
        }

        private async void CreateDavResource()
        {
            _testConnectionCommand.SetCanExecute(false);
            _createDavResourceCommand.SetCanExecute(false);
            try
            {
                CalenderUrl = await OptionTasks.CreateDavResource(_model, CalenderUrl);
            }
            catch (Exception x)
            {
                s_logger.Error("Exception while adding a DAV resource.", x);
                string message = null;
                for (Exception ex = x; ex != null; ex = ex.InnerException)
                    message += ex.Message + Environment.NewLine;
                MessageBox.Show(message, OptionTasks.CreateDavResourceCaption);
            }
            finally
            {
                _testConnectionCommand.SetCanExecute(true);
                _createDavResourceCommand.SetCanExecute(true);
            }
        }

        public IViewOptions ViewOptions { get; }
    }
}