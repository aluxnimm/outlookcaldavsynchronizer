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
using System.Reflection;
using System.Windows.Forms;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Utilities;
using log4net;
using log4net.Config;
using Microsoft.Office.Core;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Outlook;
using CalDavSynchronizer.Ui;
using Exception = System.Exception;

namespace CalDavSynchronizer
{
  public partial class ThisAddIn
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private CalDavSynchronizerToolBar _calDavSynchronizerToolBar; // Pierre-Marie Baty -- only for Outlook < 2010
    private Explorers _explorers;
    private Explorer _activeExplorer;
    public static ComponentContainer ComponentContainer { get; private set; }

    public static event EventHandler SynchronizationFailedWhileReportsFormWasNotVisible;
    public static event EventHandler<SchedulerStatusEventArgs> StatusChanged;
    private void OnSynchronizationFailedWhileReportsFormWasNotVisible ()
    {
      var handler = SynchronizationFailedWhileReportsFormWasNotVisible;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    private async void ThisAddIn_Startup (object sender, EventArgs e)
    {
      try
      {
        // Sometimes outlook raises this event multiple times (seems to be a bug)
        if (ComponentContainer != null)
          return;

        XmlConfigurator.Configure();
        s_logger.Info ("Startup entered.");

        ComponentContainer = new ComponentContainer (Application);
        ComponentContainer.SynchronizationFailedWhileReportsFormWasNotVisible += ComponentContainer_SynchronizationFailedWhileReportsFormWasNotVisible;
        ComponentContainer.StatusChanged += ComponentContainer_StatusChanged;
        ((ApplicationEvents_Event) Application).Quit += ThisAddIn_Quit;

        _explorers = Application.Explorers;
        _explorers.NewExplorer += Explorers_NewExplorer;

        AddToolBarIfRequired();

        ComponentContainer.EnsureSynchronizationContext();

        s_logger.Info ("Initializing component container.");
        await ComponentContainer.Initialize();

        s_logger.Info ("Startup exiting.");
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void AddToolBarIfRequired()
    {
      if (!IsOutlookVersionSmallerThan2010)
        return;

      _activeExplorer = Application.ActiveExplorer();

      if (_activeExplorer != null)
      {
        // For every explorer there has to be a toolbar created, but only the first toolbar is allowed to have wired events and only a reference to the first toolbar is stored
        var calDavSynchronizerToolBar = new CalDavSynchronizerToolBar(_activeExplorer, ComponentContainer, missing, _calDavSynchronizerToolBar == null);
        calDavSynchronizerToolBar.Settings = ComponentContainer.LoadToolBarSettings();
        if (_calDavSynchronizerToolBar == null)
        {
          _calDavSynchronizerToolBar = calDavSynchronizerToolBar;
          ((ExplorerEvents_10_Event) _activeExplorer).Close += FirstExplorer_Close;
        }
      }
    }

    private void FirstExplorer_Close ()
    {
      ComponentContainer.SaveToolBarSettings(_calDavSynchronizerToolBar.Settings);
    }

    private void Explorers_NewExplorer(Explorer newExplorer)
    {
      try
      {
        newExplorer.Activate();
        AddToolBarIfRequired();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException(x, s_logger);
      }
    }

    private void ComponentContainer_StatusChanged (object sender, Scheduling.SchedulerStatusEventArgs e)
    {
      StatusChanged?.Invoke (null, e);
    }

    private void ThisAddIn_Quit ()
    {
      ComponentContainer.Dispose();
    }

    void ComponentContainer_SynchronizationFailedWhileReportsFormWasNotVisible (object sender, EventArgs e)
    {
      OnSynchronizationFailedWhileReportsFormWasNotVisible();
    }

    public static bool IsOutlookVersionSmallerThan2010
    {
      get { return Convert.ToInt32 (Globals.ThisAddIn.Application.Version.Split (new[] { '.' })[0]) < 14; }
    }

    private void ThisAddIn_Shutdown (object sender, EventArgs e)
    {
    }

    protected override IRibbonExtension[] CreateRibbonObjects ()
    {
      return new IRibbonExtension[] { new CalDavSynchronizerRibbon() };
    }

    #region VSTO generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup ()
    {
      Startup += ThisAddIn_Startup;
      Shutdown += ThisAddIn_Shutdown;
    }

    #endregion
  }
}