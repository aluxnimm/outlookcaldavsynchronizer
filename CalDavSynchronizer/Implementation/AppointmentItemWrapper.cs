using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  public class AppointmentItemWrapper : IDisposable
  {
    public AppointmentItem Inner { get; private set; }
    private Func<string, AppointmentItem> _load;

    public AppointmentItemWrapper (AppointmentItem inner, Func<string, AppointmentItem> load)
    {
      _load = load;
      Inner = inner;
    }

    public void SaveAndReload ()
    {
      Inner.Save();
      var entryId = Inner.EntryID;
      DisposeInner();
      Thread.MemoryBarrier();
      Inner = _load (entryId);
    }

    private void DisposeInner ()
    {
      Marshal.FinalReleaseComObject (Inner);
      Inner = null;
    }

    public void Dispose ()
    {
      DisposeInner();
      _load = null;
    }
  }
}