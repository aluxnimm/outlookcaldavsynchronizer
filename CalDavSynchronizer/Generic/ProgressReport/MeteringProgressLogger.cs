using System;
using System.Diagnostics;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  public class MeteringProgressLogger : IProgressLogger
  {
    private readonly Stopwatch _totalTime;
    private int _count = 1;
    private readonly int _completedCount;

    public MeteringProgressLogger (int completedCount)
    {
      _completedCount = completedCount;
      _totalTime = Stopwatch.StartNew();
    }

    public void Dispose ()
    {
      _totalTime.Stop();

      if (_count == 0)
        _count = 1;

      Debug.WriteLine ("Step Total Time: {0} , per step: {1} . TotalSteps: {2} AnnouncedSteps: {3}", _totalTime.Elapsed, _totalTime.Elapsed.TotalMilliseconds / _count, _count, _completedCount);
    }

    public void Increase ()
    {
      _count++;
    }

    public void IncreaseBy (int value)
    {
      _count += value;
    }
  }
}