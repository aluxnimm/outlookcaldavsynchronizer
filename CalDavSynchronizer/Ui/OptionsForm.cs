// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui
{
  public partial class OptionsForm : Form
  {
    private readonly NameSpace _session;

    public OptionsForm (NameSpace session)
    {
      InitializeComponent();
      _session = session;
    }


    public static bool EditOptions (NameSpace session, Options[] options, out Options[] changedOptions)
    {
      changedOptions = null;
      var form = new OptionsForm (session);
      form.OptionsList = options;
      var shouldSave = form.ShowDialog() == DialogResult.OK;
      if (shouldSave)
        changedOptions = form.OptionsList;
      return shouldSave;
    }

    private Options[] OptionsList
    {
      get
      {
        return _tabControl.TabPages
            .Cast<TabPage>()
            .Select (tp => ((OptionsDisplayControl) tp.Controls[0]).Options)
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

    private TabPage AddTabPage (Options options)
    {
      var optionsControl = new OptionsDisplayControl (_session);

      var tabPage = new TabPage (options.Name);
      _tabControl.TabPages.Add (tabPage);

      optionsControl.DeletionRequested += delegate { _tabControl.TabPages.Remove (tabPage); };
      optionsControl.ProfileNameChanged += delegate (object sender, string e)
      {
        tabPage.Text = e;
      };

      optionsControl.InactiveChanged += delegate (object sender, bool inactive)
      {
        if (inactive)
          tabPage.ImageKey = "inactive";
        else
          tabPage.ImageKey = null;
      };

      optionsControl.CopyRequested += delegate
      {
        var newOptions = optionsControl.Options;
        newOptions.Name += " (Copy)";
        var newPage = AddTabPage (newOptions);
        _tabControl.SelectedTab = newPage;
      };

      optionsControl.Options = options;
      tabPage.Controls.Add (optionsControl);
      optionsControl.Dock = DockStyle.Fill;
      return tabPage;
    }


    private void OkButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void _addProfileButton_Click (object sender, EventArgs e)
    {
      AddTabPage (Options.CreateDefault (string.Empty, string.Empty));
    }
  }
}