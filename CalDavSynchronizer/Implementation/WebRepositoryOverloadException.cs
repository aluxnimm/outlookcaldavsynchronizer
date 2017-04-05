using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.EntityRepositories;

namespace CalDavSynchronizer.Implementation
{
  public class WebRepositoryOverloadException : RepositoryOverloadException
  {
    public WebRepositoryOverloadException(DateTime? retryAfter, Exception innerException)
      : base($"The respository is overloaded.{(retryAfter != null ? $"Reftry after '{retryAfter}'" : string.Empty)} ", innerException)
    {
      RetryAfter = retryAfter;
    }

    public DateTime? RetryAfter { get; }
  }
}
