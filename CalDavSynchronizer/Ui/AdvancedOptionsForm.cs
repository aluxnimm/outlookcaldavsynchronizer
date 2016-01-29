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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Ui
{
  public partial class AdvancedOptionsForm : Form
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private MappingConfigurationBase _mappingConfiguration;
    private readonly Func<MappingConfigurationBase, MappingConfigurationBase> _coerceMappingConfiguration;
    private readonly IConfigurationFormFactory _configurationFormFactory;

    public AdvancedOptionsForm (
        Func<MappingConfigurationBase, MappingConfigurationBase> coerceMappingConfiguration,
        IConfigurationFormFactory configurationFormFactory)
    {
      _configurationFormFactory = configurationFormFactory;
      _coerceMappingConfiguration = coerceMappingConfiguration;
      InitializeComponent();
    }

    public AdvancedOptions Options
    {
      get
      {
        return new AdvancedOptions (
            _closeConnectionAfterEachRequestCheckBox.Checked,
            _preemptiveAuthenticationCheckBox.Checked,
            new ProxyOptions()
            {
                ProxyUseDefault = _useSystemProxyCheckBox.Checked,
                ProxyUseManual = _useManualProxyCheckBox.Checked,
                ProxyUrl = _proxyUrlTextBox.Text,
                ProxyUserName = _userNameTextBox.Text,
                ProxyPassword = _passwordTextBox.Text
            },
            _mappingConfiguration);
      }
      set
      {
        _closeConnectionAfterEachRequestCheckBox.Checked = value.CloseConnectionAfterEachRequest;
        _preemptiveAuthenticationCheckBox.Checked = value.PreemptiveAuthentication;
        _useSystemProxyCheckBox.Checked = value.ProxyOptions.ProxyUseDefault;
        _useManualProxyCheckBox.Checked = value.ProxyOptions.ProxyUseManual;
        _proxyUrlTextBox.Text = value.ProxyOptions.ProxyUrl;
        _userNameTextBox.Text = value.ProxyOptions.ProxyUserName;
        _passwordTextBox.Text = value.ProxyOptions.ProxyPassword;
        _manualProxyGroupBox.Enabled = value.ProxyOptions.ProxyUseManual;

        _mappingConfiguration = value.MappingConfiguration;
      }
    }

    private void OkButton_Click (object sender, EventArgs e)
    {
      StringBuilder errorMessageBuilder = new StringBuilder();
      if (_useManualProxyCheckBox.Checked && !ValidateProxyUrl (errorMessageBuilder))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The Proxy Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        DialogResult = DialogResult.None;
      }
      else
      {
        DialogResult = DialogResult.OK;
      }
    }

    private void _useManualProxyCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      if (_useManualProxyCheckBox.Checked)
        _useSystemProxyCheckBox.Checked = false;
      _manualProxyGroupBox.Enabled = _useManualProxyCheckBox.Checked;
    }

    private void _useSystemProxyCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      if (_useSystemProxyCheckBox.Checked)
        _useManualProxyCheckBox.Checked = false;
    }

    private bool ValidateProxyUrl (StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace (_proxyUrlTextBox.Text))
      {
        errorMessageBuilder.AppendLine ("- The Proxy Url is empty.");
        return false;
      }

      if (_proxyUrlTextBox.Text.Trim() != _proxyUrlTextBox.Text)
      {
        errorMessageBuilder.AppendLine ("- The Proxy Url cannot end/start with whitespaces.");
        result = false;
      }

      try
      {
        new Uri (_proxyUrlTextBox.Text).ToString();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The Proxy Url is not a well formed Url. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
    }

    private void _mappingConfigurationButton_Click (object sender, EventArgs e)
    {
      try
      {
        var mappingConfiguration = _coerceMappingConfiguration (_mappingConfiguration);
        if (mappingConfiguration != null)
        {
          var configurationForm = mappingConfiguration.CreateConfigurationForm (_configurationFormFactory);
          if (configurationForm.Display())
            _mappingConfiguration = configurationForm.Options;
        }
        else
        {
          MessageBox.Show ("Mapping configuration not available.");
          return;
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }
  }
}