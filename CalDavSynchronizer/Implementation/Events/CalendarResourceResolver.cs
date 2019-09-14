using System;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Implementation.Events
{
  class CalendarResourceResolver : ICalendarResourceResolver
  {
    private readonly ICalDavDataAccess _calDavDataAccess;

    public CalendarResourceResolver(ICalDavDataAccess calDavDataAccess)
    {
      _calDavDataAccess = calDavDataAccess ?? throw new ArgumentNullException(nameof(calDavDataAccess));
    }

    public async Task<Uri> GetResourceUriOrNull(string displayName)
    {
      return await _calDavDataAccess.GetResourceUriOrNull(displayName);
    }
  }
}