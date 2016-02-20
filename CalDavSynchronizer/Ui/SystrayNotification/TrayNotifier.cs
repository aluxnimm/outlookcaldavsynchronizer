using System;
using System.Drawing;
using System.Windows.Forms;
using CalDavSynchronizer.Properties;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification
{
  class TrayNotifier : IDisposable
  {
    private readonly NotifyIcon _nofifyIcon;
    private readonly ICalDavSynchronizerCommands _calDavSynchronizerCommands;
    

    public TrayNotifier (ICalDavSynchronizerCommands calDavSynchronizerCommands)
    {
      if (calDavSynchronizerCommands == null)
        throw new ArgumentNullException (nameof (calDavSynchronizerCommands));
      _calDavSynchronizerCommands = calDavSynchronizerCommands;

      var trayMenu = new ContextMenu();
      trayMenu.MenuItems.Add ("Synchronize now", delegate { _calDavSynchronizerCommands.SynchronizeNowNoThrow(); });
      trayMenu.MenuItems.Add ("Reports", delegate { _calDavSynchronizerCommands.ShowReportsNoThrow(); });
      trayMenu.MenuItems.Add ("Status", delegate { _calDavSynchronizerCommands.ShowProfileStatusesNoThrow(); });
      trayMenu.MenuItems.Add ("-");
      trayMenu.MenuItems.Add ("Synchronization profiles", delegate { _calDavSynchronizerCommands.ShowOptionsNoThrow(); });
      trayMenu.MenuItems.Add ("General options", delegate { _calDavSynchronizerCommands.ShowGeneralOptionsNoThrow (); });
      trayMenu.MenuItems.Add ("-");
      trayMenu.MenuItems.Add ("About", delegate { _calDavSynchronizerCommands.ShowAboutNoThrow (); });

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
      _calDavSynchronizerCommands.ShowProfileStatusesNoThrow();
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

    public void Dispose ()
    {
      _nofifyIcon.Visible = false;
      _nofifyIcon.Icon = null;
      _nofifyIcon.Dispose();
    }
  }
}