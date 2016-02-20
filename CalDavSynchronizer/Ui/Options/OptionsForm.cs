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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class OptionsForm : Form
  {
    private readonly IOptionsDisplayControlFactory _optionsDisplayControlFactory;

    public OptionsForm (NameSpace session, Func<Guid, string> profileDataDirectoryFactory, bool fixInvalidSettings)
    {
      InitializeComponent();
      _optionsDisplayControlFactory =
          new OptionsDisplayControlFactory (session, profileDataDirectoryFactory, fixInvalidSettings);
    }

    public Contracts.Options[] OptionsList
    {
      get
      {
        return _tabControl.TabPages
            .Cast<TabPage>()
            .Select (tp => ((IOptionsDisplayControl) tp.Controls[0]).Options)
            .ToArray();
      }
      set
      {
        foreach (var options in value)
        {
          AddTabPage (options);
        }
      }
    }

    private TabPage AddTabPage (Contracts.Options options)
    {
      var optionsControl = _optionsDisplayControlFactory.Create (options);

      var tabPage = new TabPage (options.Name);
      _tabControl.TabPages.Add (tabPage);

      optionsControl.DeletionRequested += delegate { _tabControl.TabPages.Remove (tabPage); };

      optionsControl.HeaderChanged += delegate (object sender, HeaderEventArgs e)
      {
        tabPage.Text = e.Name;

        switch (e.FolderItemType)
        {
          case OlItemType.olAppointmentItem:
            if (e.IsInactive)
              tabPage.ImageKey = "AppointmentDisabled";
            else
              tabPage.ImageKey = "Appointment";
            break;
          case OlItemType.olTaskItem:
            if (e.IsInactive)
              tabPage.ImageKey = "TaskDisabled";
            else
              tabPage.ImageKey = "Task";
            break;
          case OlItemType.olContactItem:
            if (e.IsInactive)
              tabPage.ImageKey = "ContactDisabled";
            else
              tabPage.ImageKey = "Contact";
            break;
          default:
            tabPage.ImageKey = null;
            break;
        }
      };

      optionsControl.CopyRequested += delegate
      {
        var newOptions = optionsControl.Options;
        newOptions.Name += " (Copy)";
        newOptions.Id = Guid.NewGuid();
        var newPage = AddTabPage (newOptions);
        _tabControl.SelectedTab = newPage;
      };

      optionsControl.Options = options;
      tabPage.Controls.Add (optionsControl.UiControl);
      optionsControl.UiControl.Dock = DockStyle.Fill;
      return tabPage;
    }

    private void OkButton_Click (object sender, EventArgs e)
    {
      TabPage firstTabPageWithError;
      string errorMessage;

      if (Validate (out errorMessage, out firstTabPageWithError))
      {
        DialogResult = DialogResult.OK;
      }
      else
      {
        MessageBox.Show (errorMessage, "Some Options contain invalid Values", MessageBoxButtons.OK, MessageBoxIcon.Error);
        if (firstTabPageWithError != null)
          _tabControl.SelectedTab = firstTabPageWithError;
      }
    }

    private bool Validate (out string errorMessage, out TabPage firstTabPageWithError)
    {
      StringBuilder errorMessageBuilder = new StringBuilder();
      bool isValid = true;
      firstTabPageWithError = null;

      foreach (TabPage tabPage in  _tabControl.TabPages)
      {
        var optionsDisplayControl = (IOptionsDisplayControl) tabPage.Controls[0];
        StringBuilder currentControlErrorMessageBuilder = new StringBuilder();

        if (!optionsDisplayControl.Validate (currentControlErrorMessageBuilder))
        {
          if (errorMessageBuilder.Length > 0)
            errorMessageBuilder.AppendLine();

          errorMessageBuilder.AppendFormat ("Profile '{0}'", optionsDisplayControl.ProfileName);
          errorMessageBuilder.AppendLine();
          errorMessageBuilder.Append (currentControlErrorMessageBuilder);

          isValid = false;
          if (firstTabPageWithError == null)
            firstTabPageWithError = tabPage;
        }
      }

      errorMessage = errorMessageBuilder.ToString();
      return isValid;
    }

    private void _addProfileButton_Click (object sender, EventArgs e)
    {
      var type = SelectOptionsDisplayTypeForm.QueryOptionsDisplayType();
      if (!type.HasValue)
        return;

      Contracts.Options options = Contracts.Options.CreateDefault (string.Empty, string.Empty);
      options.DisplayType = type.Value;
      options.ServerAdapterType = (type == OptionsDisplayType.Google)
        ? ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth
        : ServerAdapterType.WebDavHttpClientBased;

      _tabControl.SelectedTab = AddTabPage (options);
    }

    public void ShowProfile (Guid value)
    {
      foreach (TabPage tabPage in _tabControl.TabPages)
      {
        var optionsDisplayControl = (IOptionsDisplayControl) tabPage.Controls[0];
        if (optionsDisplayControl.ProfileId == value)
        {
          _tabControl.SelectedTab = tabPage;
          return;
        }
      }
    }
  }
}