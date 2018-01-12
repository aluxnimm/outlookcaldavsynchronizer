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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Globalization;

namespace CalDavSynchronizer.Ui
{
  public partial class GetNewVersionForm : Form
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private static Process _latestSetupProcess;
    private readonly Uri _newVersionDownloadUrl;
    private readonly string _archivePath;

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

    public GetNewVersionForm (
      string whatsNew, 
      Version newVersion, 
      Uri newVersionDownloadUrl,
      bool isInstallNewVersionEnabled)
    {
      InitializeComponent();

      _newVersionDownloadUrl = newVersionDownloadUrl;
      _currentVersionLabel.Text = string.Format (_currentVersionLabel.Text, Assembly.GetExecutingAssembly().GetName().Version);
      _captionLabel.Text = string.Format (_captionLabel.Text, newVersion);
      _newFeaturesTextBox.Text = whatsNew;

      _logoPictureBox.Image = Properties.Resources.ApplicationLogoLarge;

      if (!isInstallNewVersionEnabled)
      {
        installButton.Visible = false;
      }
      _archivePath = Path.GetTempFileName();
      _progressBar.Visible = false;
    }

    private void btnOK_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void _downloadNewVersionLinkLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start (_newVersionDownloadUrl.ToString());
    }

    private void _doNotCheckForNewerVersionsLinkLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      OnTurnOffCheckForNewerVersions();
    }

    private void _ignoreThisVersionLinkLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      OnIgnoreThisVersion();
    }

    private void client_DownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e)
    {
      double bytesIn = double.Parse (e.BytesReceived.ToString());
      double totalBytes = double.Parse (e.TotalBytesToReceive.ToString());
      double percentage = bytesIn / totalBytes * 100;
      _progressBar.Value = int.Parse (Math.Truncate (percentage).ToString());
    }

    private void client_DownloadFileCompleted (object sender, AsyncCompletedEventArgs e)
    {
      try
      {
        
        var extractDirectory = Path.Combine (Path.GetTempPath(), "CalDavSynchronizer", Guid.NewGuid().ToString());
        Directory.CreateDirectory (extractDirectory);
        ZipFile.ExtractToDirectory (_archivePath, extractDirectory);
        File.Delete (_archivePath);
        MessageBox.Show (
               Strings.Get($"You need to restart Outlook after installing the new version!"),
               Strings.Get($"Outlook restart required"),
               MessageBoxButtons.OK,
               MessageBoxIcon.Information);

        // process hast to be a GC root to prevent it from being garbage collected.
        var msiFile = Path.Combine (extractDirectory, "CalDavSynchronizer.Setup.msi");
        _latestSetupProcess = Process.Start ("msiexec.exe", "/i \"" + msiFile + "\" /passive");
        if (_latestSetupProcess != null)
        {
          _latestSetupProcess.EnableRaisingEvents = true;
          _latestSetupProcess.Exited += delegate
          {
            try
            {
              _latestSetupProcess = null;
              Process.Start (WebResourceUrls.ReadMeFileDownloadSite.ToString());
            }
            catch (Exception x)
            {
              s_logger.Error ("Error while downloading readme.md", x);
            }
          };
        }

        DialogResult = DialogResult.OK;
      }
      catch (Exception ex)
      {
        s_logger.Warn ("Can't extract new version", ex);
        MessageBox.Show (Strings.Get($"Can't extract new version!"), Strings.Get($"CalDav Synchronizer download failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
    private void installButton_Click (object sender, EventArgs e)
    {
      try
      {
        using (var client = HttpUtility.CreateWebClient())
        {
          client.DownloadProgressChanged += client_DownloadProgressChanged;
          client.DownloadFileCompleted += client_DownloadFileCompleted;
          _progressBar.Visible = true;
          client.DownloadFileAsync (_newVersionDownloadUrl, _archivePath);
        }
      }
      catch (Exception ex)
      {
        s_logger.Warn ("Can't download new version", ex);
        MessageBox.Show (Strings.Get($"Can't download new version!"), Strings.Get($"CalDav Synchronizer download failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}