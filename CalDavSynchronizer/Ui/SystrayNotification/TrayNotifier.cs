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
    private SyncronizationRunResult _worstRunResult;
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
      _nofifyIcon.Icon = Resources.Ok;

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
      var currentResult =
          report.HasErrors
              ? SyncronizationRunResult.Error
              : report.HasWarnings
                  ? SyncronizationRunResult.Warning
                  : SyncronizationRunResult.Ok;


      _worstRunResult = (SyncronizationRunResult) Math.Max ((int) _worstRunResult, (int) currentResult);

      switch (_worstRunResult)
      {
        case SyncronizationRunResult.Ok:
          _nofifyIcon.Icon = Resources.Ok;
          break;
        case SyncronizationRunResult.Warning:
          _nofifyIcon.Icon = SystemIcons.Warning;
          break;
        case SyncronizationRunResult.Error:
          _nofifyIcon.Icon = SystemIcons.Error;
          break;
        default:
          _nofifyIcon.Icon = SystemIcons.Question;
          break;
      }

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