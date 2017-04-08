using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.Synchronization;

namespace CalDavSynchronizer.Implementation
{
  public class ExceptionHandlingStrategy : IExceptionHandlingStrategy
  {
    public bool DoesAbortSynchronization(Exception x)
    {
      return x is WebRepositoryOverloadException;
    }
  }
}
