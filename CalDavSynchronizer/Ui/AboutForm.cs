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
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Ui
{
  public partial class AboutForm : Form
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private readonly Action _checkForUpdatesActionAsync;

    public AboutForm (Action checkForUpdatesActionAsync)
    {
      _checkForUpdatesActionAsync = checkForUpdatesActionAsync;
      InitializeComponent();

      btnOK.Text = Strings.Get($"OK");
      label1.Text = Strings.Get($"Team:");
      _linkLabelPayPal.Text = Strings.Get($"Donate with PayPal");
      _linkLabelHelp.Text = Strings.Get($"Documentation and Tutorials");
      _checkForUpdatesButton.Text = Strings.Get($"Check for Updates");
      Text = Strings.Get($"About");

      _versionLabel.Text = Strings.Get($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

      this._linkLabelProject.Text = WebResourceUrls.ProjectHomeSite.ToString();

      _linkLabelTeamMembers.LinkClicked += _linkLabelTeamMembers_LinkClicked;
      _linkLabelTeamMembers.Text = string.Empty;
      AddTeamMember ("Alexander Nimmervoll", "http://sourceforge.net/u/nimm/profile/");
      AddTeamMember ("Gerhard Zehetbauer", "http://sourceforge.net/u/nertsch/profile/");
      _logoPictureBox.Image = Properties.Resources.ApplicationLogoLarge;
    }

    public sealed override string Text
    {
      get => base.Text;
      set => base.Text = value;
    }

    private void _linkLabelTeamMembers_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start ((string) e.Link.LinkData);
    }


    private void AddTeamMember (string name, string memberHome)
    {
      if (_linkLabelTeamMembers.Text != string.Empty)
        _linkLabelTeamMembers.Text += ", ";
      var start = _linkLabelTeamMembers.Text.Length;
      _linkLabelTeamMembers.Text += name;
      _linkLabelTeamMembers.Links.Add (start, name.Length, memberHome);
    }

    private void btnOK_Click (object sender, EventArgs e)
    {
      DialogResult = System.Windows.Forms.DialogResult.OK;
    }

    private void _linkLabelProject_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start (_linkLabelProject.Text);
    }

    private void linkLabelPayPal_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start (WebResourceUrls.DonationSite.ToString());
    }

    private void linkLabelHelp_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start (WebResourceUrls.HelpSite.ToString());
    }

    private void CheckForUpdatesButton_Click (object sender, EventArgs e)
    {
      try
      {
        ComponentContainer.EnsureSynchronizationContext ();
        _checkForUpdatesActionAsync ();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }
  }
}