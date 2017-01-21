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
using System.Linq;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.ProfileTypes;

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
      _logoSarenetPictureBox.Image = Properties.Resources.logo_sarenet;
      _logoLandmarksPictureBox.Image = Properties.Resources.logo_landmarks;
      _logoSogoPictureBox.Image = Properties.Resources.logo_sogo;
      _logoCozyPictureBox.Image = Properties.Resources.logo_cozy;
      _logoNextCloudPictureBox.Image = Properties.Resources.logo_nextcloud;
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public static IProfileType QueryProfileType (IReadOnlyCollection<IProfileType> profileTypes)
    {
      var form = new SelectOptionsDisplayTypeForm();
      if (form.ShowDialog() == DialogResult.OK)
      {
        if (form._genericTypeRadioButton.Checked)
          return profileTypes.Single(p => p is GenericProfile);
        if (form._googleTypeRadionButton.Checked)
          return profileTypes.Single(p => p is GoogleProfile);
        if (form._fruuxTypeRadioButton.Checked)
          return profileTypes.Single(p => p is FruuxProfile);
        if (form._posteoTypeRadioButton.Checked)
          return profileTypes.Single(p => p is PosteoProfile);
        if (form._yandexTypeRadioButton.Checked)
          return profileTypes.Single(p => p is YandexProfile);
        if (form._gmxCalendarTypeRadioButton.Checked)
          return profileTypes.Single(p => p is GmxCalendarProfile);
        if (form._sarenetTypeRadioButton.Checked)
          return profileTypes.Single(p => p is SarenetProfile);
        if (form._landmarksTypeRadioButton.Checked)
          return profileTypes.Single(p => p is LandmarksProfile);
        if (form._sogoTypeRadioButton.Checked)
          return profileTypes.Single(p => p is SogoProfile);
        if (form._cozyTypeRadioButton.Checked)
          return profileTypes.Single(p => p is CozyProfile);
        if (form._nextCloudTypeRadioButton.Checked)
          return profileTypes.Single(p => p is NextcloudProfile);
      }

      return null;
    }
  }
}