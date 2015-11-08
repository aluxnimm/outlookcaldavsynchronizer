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

namespace CalDavSynchronizer.Ui
{
  public partial class AdvancedSettingsForm : Form
  {
    public bool closeConnectionAfterEachRequest { get; private set; }
    
    public ProxyOptions ProxyOptions
    {
      set
      {
        _useSystemProxyCheckBox.Checked = value.ProxyUseDefault;
        _useManualProxyCheckBox.Checked = value.ProxyUseManual;
        _proxyUrlTextBox.Text = value.ProxyUrl;
        _userNameTextBox.Text = value.ProxyUserName;
        _passwordTextBox.Text = value.ProxyPassword;
      }
      get
      {
        return new ProxyOptions()
               {
                   ProxyUseDefault = _useSystemProxyCheckBox.Checked,
                   ProxyUseManual = _useManualProxyCheckBox.Checked,
                   ProxyUrl = _proxyUrlTextBox.Text,
                   ProxyUserName = _userNameTextBox.Text,
                   ProxyPassword = _passwordTextBox.Text
               };
      }
    }
    public AdvancedSettingsForm (bool closeConnectionAfterEachRequest, ProxyOptions options)
    {
      InitializeComponent();
      _closeConnectionAfterEachRequestCheckBox.Checked = closeConnectionAfterEachRequest;
      ProxyOptions = options;
      _manualProxyGroupBox.Enabled = options.ProxyUseManual;
     
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
      closeConnectionAfterEachRequest = _closeConnectionAfterEachRequestCheckBox.Checked;
      DialogResult = DialogResult.OK;
    }

    private void _useManualProxyCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (_useManualProxyCheckBox.Checked) _useSystemProxyCheckBox.Checked = false;
      _manualProxyGroupBox.Enabled = _useManualProxyCheckBox.Checked;
    }

    private void _useSystemProxyCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (_useSystemProxyCheckBox.Checked) _useManualProxyCheckBox.Checked = false;
    }
  }
}
