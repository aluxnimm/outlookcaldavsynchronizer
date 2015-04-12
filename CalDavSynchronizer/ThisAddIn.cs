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
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using log4net;
using log4net.Config;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Office = Microsoft.Office.Core;

namespace CalDavSynchronizer
{
  public partial class ThisAddIn
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    internal static Scheduler Scheduler { get; private set; }
    internal static IOptionsDataAccess OptionsDataAccess { get; private set; }
    internal static NameSpace Session { get; private set; }


    private void ThisAddIn_Startup (object sender, EventArgs e)
    {
      InitializeSynchronizer ();
    }

    private void InitializeSynchronizer ()
    {
      try
      {
        XmlConfigurator.Configure ();

        Session = Application.Session;

        s_logger.Info ("Startup...");

        var applicationDataDirectory = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

        OptionsDataAccess = new OptionsDataAccess (
            Path.Combine (
                applicationDataDirectory,
                "options.xml"
                ));
        Scheduler = new Scheduler (Application.Session, applicationDataDirectory);
        Scheduler.SetOptions (OptionsDataAccess.LoadOptions ());
      }
      catch (Exception x)
      {
        s_logger.Error (string.Empty, x);
        throw;
      }

      s_logger.Info ("Startup finnished");
    }

    private void ThisAddIn_Shutdown (object sender, EventArgs e)
    {

    }

    protected override Microsoft.Office.Tools.Ribbon.IRibbonExtension[] CreateRibbonObjects ()
    {
      return new Microsoft.Office.Tools.Ribbon.IRibbonExtension[] { new CalDavSynchronizerRibbon () };
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