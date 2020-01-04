using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Scheduling
{
  class DateTimeEqualityComparer : IEqualityComparer<DateTime>
  {
    public bool Equals(DateTime x, DateTime y)
    {
      var xUtc = x.Kind == DateTimeKind.Utc ? x : x.ToUniversalTime();
      var yUtc = y.Kind == DateTimeKind.Utc ? y : y.ToUniversalTime();

      return xUtc == yUtc;
    }

    public int GetHashCode(DateTime obj)
    {
      return (obj.Kind == DateTimeKind.Utc ? obj : obj.ToUniversalTime()).GetHashCode();
    }
  }
}
