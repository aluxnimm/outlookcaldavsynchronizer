using System;
using System.Threading;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  public class AppointmentItemWrapper : IDisposable
  {
    public AppointmentItem Inner { get; private set; }
    private readonly Func<string, AppointmentItem> _load;

    public AppointmentItemWrapper (AppointmentItem inner, Func<string, AppointmentItem> load)
    {
      _load = load;
      Inner = inner;
    }

    public void SaveAndReload ()
    {
      Inner.Save();
      var entryId = Inner.EntryID;
      Inner = null;
      Thread.MemoryBarrier();
      Inner = _load(entryId);
    }

    public void Dispose ()
    {
      Inner = null;
    }
  }
}