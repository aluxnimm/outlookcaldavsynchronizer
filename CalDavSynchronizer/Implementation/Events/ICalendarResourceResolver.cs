using System;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Implementation.Events
{
    public interface ICalendarResourceResolver
    {
        Task<Uri> GetResourceUriOrNull(string displayName);
    }
}