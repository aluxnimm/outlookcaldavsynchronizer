using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenSync.Synchronization
{
  public static class EnumerableAdditions
  {
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
      foreach (var value in values)
        action(value);
    }
  }
}
