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
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class SelectOptionsDisplayTypeForm : Form
  {
    public SelectOptionsDisplayTypeForm ()
    {
      InitializeComponent();
      _logoGooglePictureBox.Image = Properties.Resources.logo_google;
      _logoFruuxPictureBox.Image = Properties.Resources.logo_fruux;
      _logoPosteoPictureBox.Image = Properties.Resources.logo_posteo;
      _logoYandexPictureBox.Image = Properties.Resources.logo_yandex;
      _logoGmxCalendarPictureBox.Image = Properties.Resources.logo_gmx;
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public static OptionsDisplayType? QueryOptionsDisplayType ()
    {
      var form = new SelectOptionsDisplayTypeForm();
      if (form.ShowDialog() == DialogResult.OK)
      {
        if (form._genericTypeRadioButton.Checked)
          return OptionsDisplayType.Generic;
        if (form._googleTypeRadionButton.Checked)
          return OptionsDisplayType.Google;
        if (form._fruuxTypeRadioButton.Checked)
          return OptionsDisplayType.Fruux;
        if (form._posteoTypeRadioButton.Checked)
          return OptionsDisplayType.Posteo;
        if (form._yandexTypeRadioButton.Checked)
          return OptionsDisplayType.Yandex;
        if (form._gmxCalendarTypeRadioButton.Checked)
          return OptionsDisplayType.GmxCalendar;
      }

      return null;
    }
  }
}