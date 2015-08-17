using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizerTestAutomation.Infrastructure;
using DDay.iCal;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Synchronization;
using NUnit.Framework;
using Rhino.Mocks;

namespace CalDavSynchronizerTestAutomation.EventRoundTrip
{
  [TestFixture]
  public class CalDavToOutlookToCalDavRoundTrip
  {
    [Test]
    public void KeepsCustomPropertiesAndUid ()
    {
      var eventData = @"
BEGIN:VCALENDAR
PRODID:-//Inverse inc./SOGo 2.2.16//EN
VERSION:2.0
BEGIN:VEVENT
UID:59E2-55170300-17-5DFC2102
SUMMARY:testmeeting
DTSTART;TZID=Europe/Vienna:20150429T110000
DTEND;TZID=Europe/Vienna:20150429T123000
X-CALDAVSYNCHRONIZER-TEST:This is a test property
END:VEVENT
END:VCALENDAR
      ";

      var roundTrippedData = ImportAndExport (eventData);

      Assert.That (roundTrippedData, Is.StringContaining ("X-CALDAVSYNCHRONIZER-TEST:This is a test property"));
      Assert.That (roundTrippedData, Is.StringContaining ("UID:59E2-55170300-17-5DFC2102"));
    }

    private static string ImportAndExport (string eventData)
    {
      ImportToOutlook (eventData);
      return ExportFromOutlook();
    }

    private static void ImportToOutlook (string eventData)
    {
      var calDavRepository = MockRepository.GenerateMock<ICalDavDataAccess>();

      var entityUri = new Uri ("/e1", UriKind.Relative);

      calDavRepository
          .Expect (r => r.GetEvents (null))
          .IgnoreArguments()
          .Return (new[] { EntityIdWithVersion.Create (entityUri, "v1") });

      calDavRepository
          .Expect (r => r.GetEntities (Arg<ICollection<Uri>>.List.Equal (new[] { entityUri })))
          .Return (new[] { EntityWithVersion.Create (entityUri, eventData) });

      var synchronizer = OutlookTestContext.CreateEventSynchronizer (SynchronizationMode.ReplicateServerIntoOutlook, calDavRepository);

      WaitForTask (synchronizer.Synchronize());
    }

    /// <remarks>
    /// Task.Wait() resp. Task.Result cannot be used, because it will deadlock!
    /// 
    /// Note: In some cases, this might lead to a deadlock: Your call to Result blocks the main thread, 
    /// thereby preventing the remainder of the async code to execute. 
    /// (see http://stackoverflow.com/questions/22628087/calling-async-method-synchronously)
    /// </remarks>
    private static void WaitForTask (Task task)
    {
      while (!task.IsCompleted)
      {
        task.Wait (100);
        Application.DoEvents();
      }
    }

    private static string ExportFromOutlook ()
    {
      string roundTrippedData = null;

      ICalDavDataAccess calDavRepository = MockRepository.GenerateMock<ICalDavDataAccess>();

      calDavRepository
          .Expect (r => r.GetEvents (null))
          .IgnoreArguments()
          .Return (new EntityIdWithVersion<Uri, string>[] { });

      calDavRepository
          .Expect (r => r.CreateEntity (null))
          .IgnoreArguments()
          .Return (EntityIdWithVersion.Create (new Uri ("http://bla.com"), "blubb"))
          .WhenCalled (a => { roundTrippedData = (string) a.Arguments[0]; });

      ISynchronizer synchronizer = OutlookTestContext.CreateEventSynchronizer (SynchronizationMode.ReplicateOutlookIntoServer, calDavRepository);
      WaitForTask (synchronizer.Synchronize());

      return roundTrippedData;
    }
  }
}