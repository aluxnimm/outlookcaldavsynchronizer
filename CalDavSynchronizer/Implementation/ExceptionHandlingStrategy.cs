using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using GenSync.Synchronization;

namespace CalDavSynchronizer.Implementation
{
  public class ExceptionHandlingStrategy : IExceptionHandlingStrategy
  {
    public bool DoesAbortSynchronization(Exception x)
    {
      return 
        x is WebRepositoryOverloadException ||
        x is HttpRequestException ||
        x is WebDavClientException ||
        x is TaskCanceledException;
    }
  }
}
