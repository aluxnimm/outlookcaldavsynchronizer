using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.Synchronization;

namespace CalDavSynchronizer.Implementation
{
  class ExceptionHandlingStrategy : IExceptionHandlingStrategy
  {
    public static readonly IExceptionHandlingStrategy Instance = new ExceptionHandlingStrategy();

    private ExceptionHandlingStrategy()
    {
    }

    public bool DoesAbortSynchronization(Exception x)
    {
      return x is WebRepositoryOverloadException;
    }
  }
}
