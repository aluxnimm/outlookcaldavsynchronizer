using System;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  internal class MeteringTotalProgressFactory : ITotalProgressFactory
  {
    public static readonly ITotalProgressFactory Instance = new NullTotalProgressFactory();

    public ITotalProgress Create (int aLoadCount, int bLoadCount)
    {
      return new MeteringTotalProgress();
    }
  }
}