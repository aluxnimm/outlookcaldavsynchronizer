using System;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  internal class MeteringTotalProgressFactory : ITotalProgressFactory
  {
    public static readonly ITotalProgressFactory Instance = new NullTotalProgressFactory();

    public ITotalProgressLogger Create (int aLoadCount, int bLoadCount)
    {
      return new MeteringTotalProgressLogger();
    }
  }
}