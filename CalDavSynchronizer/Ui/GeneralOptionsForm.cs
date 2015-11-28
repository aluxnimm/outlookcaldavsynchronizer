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
  public partial class GeneralOptionsForm : Form, IConfigurationForm<GeneralOptions>
  {
    public GeneralOptionsForm ()
    {
      InitializeComponent();
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public bool Display ()
    {
      return ShowDialog() == DialogResult.OK;
    }

    public GeneralOptions Options
    {
      get
      {
        return new GeneralOptions
               {
                   ShouldCheckForNewerVersions = _checkForNewerVersionsCheckBox.Checked,
                   StoreAppDataInRoamingFolder = _storeDataInRoamingFolderCheckBox.Checked,
                   DisableCertificateValidation = _disableCertificateValidationCheckbox.Checked,
                   EnableTls12 = _enableTls12Checkbox.Checked,
                   EnableSsl3 = _enableSsl3Checkbox.Checked,
                   FixInvalidSettings = _fixInvalidSettingsCheckBox.Checked,
                   DisplayAllProfilesAsGeneric = _displayAllProfilesAsGenericCheckBox.Checked
               };
      }
      set
      {
        _checkForNewerVersionsCheckBox.Checked = value.ShouldCheckForNewerVersions;
        _storeDataInRoamingFolderCheckBox.Checked = value.StoreAppDataInRoamingFolder;
        _disableCertificateValidationCheckbox.Checked = value.DisableCertificateValidation;
        _enableTls12Checkbox.Checked = value.EnableTls12;
        _enableSsl3Checkbox.Checked = value.EnableSsl3;
        _fixInvalidSettingsCheckBox.Checked = value.FixInvalidSettings;
        _displayAllProfilesAsGenericCheckBox.Checked = value.DisplayAllProfilesAsGeneric;
      }
    }
  }
}