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