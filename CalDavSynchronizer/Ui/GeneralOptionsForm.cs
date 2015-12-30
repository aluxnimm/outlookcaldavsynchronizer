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
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui
{
  public partial class GeneralOptionsForm : Form, IConfigurationForm<GeneralOptions>
  {
    private readonly IList<Item<ReportLogMode>> _availableReportLogModes = new List<Item<ReportLogMode>>()
                                                                           {
                                                                               new Item<ReportLogMode> (ReportLogMode.OnlyWithErrors, "Only sync runs with errors"),
                                                                               new Item<ReportLogMode> (ReportLogMode.WarningsOrErrors, "Sync runs with errors or warnings"),
                                                                               new Item<ReportLogMode> (ReportLogMode.All, "All sync runs")
                                                                           };

    private readonly IList<Item<ReportPopupMode>> _availableReportPopupModes = new List<Item<ReportPopupMode>>()
                                                                               {
                                                                                   new Item<ReportPopupMode> (ReportPopupMode.NoPopup, "No"),
                                                                                   new Item<ReportPopupMode> (ReportPopupMode.JustErrors, "Just errors"),
                                                                                   new Item<ReportPopupMode> (ReportPopupMode.WarningsAndErrors, "Errors and warnings")
                                                                               };

    private enum ReportLogMode
    {
      OnlyWithErrors,
      WarningsOrErrors,
      All
    }

    private enum ReportPopupMode
    {
      NoPopup,
      JustErrors,
      WarningsAndErrors
    }

    public GeneralOptionsForm ()
    {
      InitializeComponent();

      Item.BindComboBox (_reportLogModeComboBox, _availableReportLogModes);
      Item.BindComboBox (_reportPopupModeComboBox, _availableReportPopupModes);
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
        var reportLogMode = ((ReportLogMode) _reportLogModeComboBox.SelectedValue);
        var reportPopupMode = ((ReportPopupMode) _reportPopupModeComboBox.SelectedValue);
        return new GeneralOptions
               {
                   ShouldCheckForNewerVersions = _checkForNewerVersionsCheckBox.Checked,
                   StoreAppDataInRoamingFolder = _storeDataInRoamingFolderCheckBox.Checked,
                   DisableCertificateValidation = _disableCertificateValidationCheckbox.Checked,
                   EnableTls12 = _enableTls12Checkbox.Checked,
                   EnableSsl3 = _enableSsl3Checkbox.Checked,
                   FixInvalidSettings = _fixInvalidSettingsCheckBox.Checked,
                   DisplayAllProfilesAsGeneric = _displayAllProfilesAsGenericCheckBox.Checked,
                   LogReportsWithoutWarningsOrErrors = reportLogMode == ReportLogMode.All,
                   LogReportsWithWarnings = reportLogMode == ReportLogMode.All || reportLogMode == ReportLogMode.WarningsOrErrors,
                   ShowReportsWithErrorsImmediately = reportPopupMode == ReportPopupMode.JustErrors || reportPopupMode == ReportPopupMode.WarningsAndErrors,
                   ShowReportsWithWarningsImmediately = reportPopupMode == ReportPopupMode.WarningsAndErrors,
                   MaxReportAgeInDays = int.Parse(_maxReportAgeInDays.Text)
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
        _reportLogModeComboBox.SelectedValue = GetReportLogMode (value);
        _reportPopupModeComboBox.SelectedValue = GetReportPopupMode (value);
        _maxReportAgeInDays.Text = value.MaxReportAgeInDays.ToString();
      }
    }

    private ReportLogMode GetReportLogMode (GeneralOptions options)
    {
      if (options.LogReportsWithoutWarningsOrErrors)
      {
        // HINT: althoug it is possible that in this case LogReportsWithWarnings is false, 
        // the UI doesnt offer that feature to just configure logging of reports without errors and warning 
        return ReportLogMode.All;
      }
      else if (options.LogReportsWithWarnings)
      {
        return ReportLogMode.WarningsOrErrors;
      }
      else
      {
        return ReportLogMode.OnlyWithErrors;
      }
    }

    private ReportPopupMode GetReportPopupMode (GeneralOptions options)
    {
      if (options.ShowReportsWithWarningsImmediately)
      {
        // HINT: althoug it is possible that in this case ShowReportsWithErrorsImmediately is false, 
        // the UI doesnt offer that feature to just show reports with warnings immediately
        return ReportPopupMode.WarningsAndErrors;
      }
      else if (options.ShowReportsWithErrorsImmediately)
      {
        return ReportPopupMode.JustErrors;
      }
      else
      {
        return ReportPopupMode.NoPopup;
      }
    }
  }
}