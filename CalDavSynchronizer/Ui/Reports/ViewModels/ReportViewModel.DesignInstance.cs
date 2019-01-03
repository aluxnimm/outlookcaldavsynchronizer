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
using System.Windows;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.Reports.ViewModels
{
  public partial class ReportViewModel : ModelBase
  {
    public static ReportViewModel DesignInstance => CreateDesignInstance();

    static SynchronizationReport SynchronizationReportDesignInstance
    {
      get
      {
        return new SynchronizationReport
               {
                   Duration = TimeSpan.FromSeconds (200),
                   ExceptionThatLeadToAbortion = CreateException ("Exception that lead to abortion"),
                   ProfileId = Guid.NewGuid(),
                   ProfileName = "Profile name",
                   ADelta = "This is the ADelta",
                   BDelta = "This is the ADelta",
                   StartTime = new DateTime (2000, 6, 6, 13, 37, 0),
                   LoadErrors = new[]
                                {
                                    new LoadError()
                                    {
                                        EntityId = "/SOGo/dav/se13m017/Calendar/personal/ef7fe2af-b38f-44d6-89aa-356ee1fc0940.ics",
                                        Error = CreateException ("Doesnt work..."),
                                        IsAEntity = false
                                    },
                                    new LoadError()
                                    {
                                        EntityId = "0000000028D820BC0737DB4C8B99236D4EE6B04E0700025B101ADEE08447AD25B60FE763ED76003C4B923C400000025B101ADEE08447AD25B60FE763ED7600797457C1F90000",
                                        Error = "Dont want to...",
                                        IsAEntity = true
                                    },
                                    new LoadError()
                                    {
                                        EntityId = "1200000028D820BC0737DB4C8B99236D4EE6B04E0700025B101ADEE08447AD25B60FE763ED76003C4B923C400000025B101ADEE08447AD25B60FE763ED7600797457C1F90000",
                                        Error = "Dont still want to...",
                                        IsAEntity = true
                                    },
                                },
                   EntitySynchronizationReports = new[]
                                                  {
                                                      new EntitySynchronizationReport()
                                                      {
                                                          AId = "0000000028D820BC0737DB4C8B99236D4EE6B04E0700025B101ADEE08447AD25B60FE763ED76003C4B923C400000025B101ADEE08447AD25B60FE763ED7600797457C1F90000",
                                                          BId = "/SOGo/dav/se13m017/Calendar/personal/ef7fe2af-b38f-44d6-89aa-356ee1fc0940.ics",
                                                          ExceptionThatLeadToAbortion = CreateException ("Strange exception"),
                                                          Errors = new[]
                                                                          {
                                                                              CreateException ("Mapping error 1"),
                                                                              "Mapping error 2",
                                                                          },
                                                          Warnings = new[]
                                                                            {
                                                                                CreateException ("Mapping warning 1"),
                                                                                "Mapping warning 2",
                                                                            }
                                                      },
                                                      new EntitySynchronizationReport()
                                                      {
                                                          AId = "ANOTHER028D820BC0737DB4C8B99236D4EE6B04E0700025B101ADEE08447AD25B60FE763ED76003C4B923C400000025B101ADEE08447AD25B60FE763ED7600797457C1F90000",
                                                          BId = "/SOGo/dav/se13m017/Calendar/personal/ef7fe2af-b38f-44d6-89aa-356ee1fc0940another.ics",
                                                          ExceptionThatLeadToAbortion = CreateException ("Another exception"),
                                                          Errors = new[]
                                                                          {
                                                                              CreateException ("Another Mapping error 1"),
                                                                              "Another Mapping error 2",
                                                                          },
                                                          Warnings = new[]
                                                                            {
                                                                                CreateException ("Mapping warning 1"),
                                                                                "Mapping warning 2",
                                                                            }
                                                      }
                                                  }
               };
      }
    }


    public static LoadError LoadErrorDesignInstance => new LoadError()
                                                       {
                                                           EntityId = "Entity 2",
                                                           Error = CreateException ("Doesnt work..."),
                                                           IsAEntity = true
                                                       };

    public static EntitySynchronizationReport EntitySynchronizationReportDesignInstance => new EntitySynchronizationReport
                                                                                           {
                                                                                               AId = "0000000028D820BC0737DB4C8B99236D4EE6B04E0700025B101ADEE08447AD25B60FE763ED76003C4B923C400000025B101ADEE08447AD25B60FE763ED7600797457C1F90000",
                                                                                               BId = "/SOGo/dav/se13m017/Calendar/personal/ef7fe2af-b38f-44d6-89aa-356ee1fc0940.ics",
                                                                                               ExceptionThatLeadToAbortion = CreateException ("Strange exception"),
                                                                                               Errors = new[]
                                                                                                               {
                                                                                                                   CreateException ("Mapping error 1"),
                                                                                                                   "Mapping error 2",
                                                                                                               },
                                                                                               Warnings = new[]
                                                                                                                 {
                                                                                                                     CreateException ("Mapping warning 1"),
                                                                                                                     "Mapping warning 2",
                                                                                                                 }
                                                                                           };

    public static ReportViewModel CreateDesignInstance (bool hasWarnings = false, bool hasErrors = false)
    {
      var reportName = SynchronizationReportName.Create (Guid.NewGuid(), new DateTime (2000, 10, 10), hasWarnings, hasErrors);

      var proxy = new ReportProxy (reportName, () => SynchronizationReportDesignInstance, "The profile name");

      return new ReportViewModel (proxy, NullSynchronizationReportRepository.Instance, NullReportViewModelParent.Instance);
    }


    private static string CreateException (string message)
    {
      Action a = () => { new Func<int> (() => { throw new Exception (message); })(); };

      try
      {
        a();
      }
      catch (Exception x)
      {
        return x.ToString();
      }
      return "Could not create exception!";
    }
  }
}