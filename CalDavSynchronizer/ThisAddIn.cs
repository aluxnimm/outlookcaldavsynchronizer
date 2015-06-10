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
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Utilities;
using log4net;
using log4net.Config;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Tools.Ribbon;
using Exception = System.Exception;
using Office = Microsoft.Office.Core;

namespace CalDavSynchronizer
{
  public partial class ThisAddIn
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    internal static Scheduler Scheduler { get; private set; }
    internal static IOptionsDataAccess OptionsDataAccess { get; private set; }
    internal static NameSpace Session { get; private set; }


    private void ThisAddIn_Startup (object sender, EventArgs e)
    {
      InitializeSynchronizer();
    }

    private void InitializeSynchronizer ()
    {
      try
      {
        XmlConfigurator.Configure();

        Session = Application.Session;
        s_logger.Info ("Startup...");

        var applicationDataDirectory = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

        OptionsDataAccess = new OptionsDataAccess (
            Path.Combine (
                applicationDataDirectory,
                GetOrCreateConfigFileName (applicationDataDirectory, Session.CurrentProfileName)
                ));
        Scheduler = new Scheduler (Application.Session, applicationDataDirectory);
        Scheduler.SetOptions (OptionsDataAccess.LoadOptions());
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
        throw;
      }

      s_logger.Info ("Startup finnished");
    }

    private static string GetOrCreateConfigFileName (string applicationDataDirectory, string profileName)
    {
      var profileDataAccess = new ProfileListDataAccess (Path.Combine (applicationDataDirectory, "profiles.xml"));
      var profiles = profileDataAccess.Load();
      var profile = profiles.FirstOrDefault (p => p.ProfileName.Equals (profileName, StringComparison.OrdinalIgnoreCase));
      if (profile == null)
      {
        profile = new ProfileEntry()
                  {
                      ProfileName = profileName,
                      ConfigFileName = string.Format ("options_{0}.xml", Guid.NewGuid())
                  };
        profiles = profiles.Union (new[] { profile }).ToArray();
        profileDataAccess.Save (profiles);
      }
      return profile.ConfigFileName;
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