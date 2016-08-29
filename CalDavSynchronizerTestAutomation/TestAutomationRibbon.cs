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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Utilities;
using CalDavSynchronizerTestAutomation.Infrastructure;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizerTestAutomation
{
  public partial class CalDavSynchronizerTestRibbon
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private void TestAutomationRibbon_Load (object sender, RibbonUIEventArgs e)
    {
      OutlookTestContext.Initialize (Globals.ThisAddIn.Application.Session);
    }

    private void StartTestsButton_Click (object sender, RibbonControlEventArgs e)
    {
      StartTests (false);
    }

    private static void StartTests (bool excludeManual)
    {
      EnsureSynchronizationContext();
      var display = new TestResultDisplay();
      display.Show();
      var runner = new TestRunner (display);
      ManualAssert.Initialize (display);
      runner.Run (Assembly.GetExecutingAssembly(), excludeManual);
    }

    /// <summary>
    /// Ensures that the syncronizationcontext is not null ( it seems to be a bug that the synchronizationcontext is null in Office Addins)
    /// </summary>
    public static void EnsureSynchronizationContext ()
    {
      if (System.Threading.SynchronizationContext.Current == null)
      {
        System.Threading.SynchronizationContext.SetSynchronizationContext (new System.Windows.Forms.WindowsFormsSynchronizationContext());
      }
    }

    private void StartTestsExcludeManualButton_Click (object sender, RibbonControlEventArgs e)
    {
      StartTests (true);
    }

    private async void ImportIcsData_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        EnsureSynchronizationContext ();

        var dataInputWindow = CreateWindowWithTextBox();
        dataInputWindow.Item1.ShowDialog();

        var entitySynchronizationLogger = new EntitySynchronizationLogger ();

        await OutlookTestContext.EventRepository.Create (
            async appointmentWrapper => await OutlookTestContext.EntityMapper.Map2To1 (
                OutlookTestContext.DeserializeICalendar (dataInputWindow.Item2.Text),
                appointmentWrapper,
                entitySynchronizationLogger),
            NullEventSynchronizationContext.Instance);

        var reportWindow = CreateWindowWithTextBox();
        reportWindow.Item2.Text = "SynchronizationReport:\r\n" + Serializer<EntitySynchronizationReport>.Serialize (entitySynchronizationLogger.GetReport());
        reportWindow.Item1.ShowDialog();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private static Tuple<Window,TextBox> CreateWindowWithTextBox ()
    {
      var window = new Window();
      window.Height = 600;
      window.Width = 800;

      var scrollViewer = new ScrollViewer();
      scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      window.Content = scrollViewer;

      var textBox = new TextBox();
      textBox.AcceptsReturn = true;
      textBox.AcceptsTab = true;
      textBox.Text = "PASTE DATA HERE";
      scrollViewer.Content = textBox;

      ElementHost.EnableModelessKeyboardInterop (window);
      return Tuple.Create(window,textBox);
    }
  }
}