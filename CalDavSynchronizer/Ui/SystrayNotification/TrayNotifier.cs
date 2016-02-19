using System;
using System.Drawing;
using System.Windows.Forms;
using CalDavSynchronizer.Properties;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification
{
  class TrayNotifier
  {
    private readonly NotifyIcon _nofifyIcon;
    public event EventHandler ShowProfileStatusesRequested;

    public TrayNotifier ()
    {
      var trayMenu = new ContextMenu();
      trayMenu.MenuItems.Add ("Exit", delegate { });

      // Create a tray icon. In this example we use a
      // standard system icon for simplicity, but you
      // can of course use your own custom icon too.
      _nofifyIcon = new NotifyIcon();
      _nofifyIcon.Text = ComponentContainer.MessageBoxTitle;
      _nofifyIcon.Icon = Resources.ApplicationIcon;
      
      // Add menu to tray icon and show it.
      _nofifyIcon.ContextMenu = trayMenu;
      _nofifyIcon.Visible = true;
      _nofifyIcon.MouseDoubleClick += _nofifyIcon_MouseDoubleClick;
    }

    private void _nofifyIcon_MouseDoubleClick (object sender, MouseEventArgs e)
    {
      OnShowProfileStatusesRequested();
    }

    public void NotifyUser (SynchronizationReport report)
    {
      if (report.HasErrors)
      {
        _nofifyIcon.ShowBalloonTip (
            10 * 1000,
            ComponentContainer.MessageBoxTitle,
            $"Syncronization profile '{report.ProfileName}' executed with error(s).",
            ToolTipIcon.Error);
      }
      else if (report.HasWarnings)
      {
        _nofifyIcon.ShowBalloonTip (
            10 * 1000,
            ComponentContainer.MessageBoxTitle,
            $"Syncronization profile '{report.ProfileName}' executed with warnings(s).",
            ToolTipIcon.Warning);
      }
    }

    private void OnShowProfileStatusesRequested ()
    {
      ShowProfileStatusesRequested?.Invoke (this, EventArgs.Empty);
    }
  }
}