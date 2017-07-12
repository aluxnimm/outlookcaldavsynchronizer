using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;
using GenSync.Synchronization;

namespace CalDavSynchronizer.Implementation
{
  public class ExceptionHandlingStrategy : IExceptionHandlingStrategy
  {
    public bool DoesGracefullyAbortSynchronization(Exception x)
    {
      return 
        x is WebRepositoryOverloadException ||
        x is TaskCanceledException ||
        x.IsTimeoutException();
    }
  }
}
