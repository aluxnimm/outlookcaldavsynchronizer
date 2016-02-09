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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using log4net;

namespace CalDavSynchronizer.Ui
{
  public partial class GetNewVersionForm : Form
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly string _newVersionDownloadUrl;

    public event EventHandler TurnOffCheckForNewerVersions;
    public event EventHandler IgnoreThisVersion;

    protected virtual void OnIgnoreThisVersion ()
    {
      var handler = IgnoreThisVersion;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    protected virtual void OnTurnOffCheckForNewerVersions ()
    {
      var handler = TurnOffCheckForNewerVersions;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public GetNewVersionForm ()
    {
      InitializeComponent();
    }

    public GetNewVersionForm (string whatsNew, Version newVersion, string newVersionDownloadUrl)
    {
      InitializeComponent();

      _newVersionDownloadUrl = newVersionDownloadUrl;
      _currentVersionLabel.Text = string.Format (_currentVersionLabel.Text, Assembly.GetExecutingAssembly().GetName().Version);
      _captionLabel.Text = string.Format (_captionLabel.Text, newVersion);
      _newFeaturesTextBox.Text = whatsNew;

      _logoPictureBox.Image = Properties.Resources.outlookcaldavsynchronizerlogoarrow;
    }

    private void btnOK_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void _downloadNewVersionLinkLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start (_newVersionDownloadUrl);
    }

    private void _doNotCheckForNewerVersionsLinkLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      OnTurnOffCheckForNewerVersions();
    }

    private void _ignoreThisVersionLinkLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      OnIgnoreThisVersion();
    }

    private void installButton_Click (object sender, EventArgs e)
    {
      try
      {
        var archivePath = Path.GetTempFileName();
        using (var client = CreateWebClient())
        {
          client.DownloadFile (new Uri (_newVersionDownloadUrl), archivePath);
        }

        var extractDirectory = Path.Combine (Path.GetTempPath(), "CalDavSynchronizer", Guid.NewGuid().ToString());
        Directory.CreateDirectory (extractDirectory);
        ZipFile.ExtractToDirectory (archivePath, extractDirectory);
        File.Delete (archivePath);

        MessageBox.Show (
            "You need to restart Outlook after installing the new version!",
            "Outlook Restart required",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        Process.Start (Path.Combine (extractDirectory, "setup.exe"));
        DialogResult = DialogResult.OK;
      }
      catch (Exception ex)
      {
        s_logger.Warn ("Can't download and extract new version", ex);
        MessageBox.Show ("Can't download and extract new version!", "CalDav Synchronizer Download failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static WebClient CreateWebClient ()
    {
      var client = new WebClient();
      var proxy = WebRequest.DefaultWebProxy;
      proxy.Credentials = CredentialCache.DefaultCredentials;
      client.Proxy = proxy;
      return client;
    }
  }
}