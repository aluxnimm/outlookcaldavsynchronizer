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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalDavSynchronizer.Ui
{
  public partial class AboutForm : Form
  {
    public AboutForm ()
    {
      InitializeComponent();
      _versionLabel.Text = string.Format (_versionLabel.Text, Assembly.GetExecutingAssembly().GetName().Version);

      _linkLabelTeamMembers.LinkClicked += _linkLabelTeamMembers_LinkClicked;
      _linkLabelTeamMembers.Text = string.Empty;
      AddTeamMember ("Alexander Nimmervoll", "http://sourceforge.net/u/nimm/profile/");
      AddTeamMember ("Gerhard Zehetbauer", "http://sourceforge.net/u/nertsch/profile/");
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
  }
}