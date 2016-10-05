using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Utilities
{
  static class ChunkedExecutor
  {
    private static async Task<TExecutionContext> ExecuteAsync<TItem, TExecutionContext>(
      TExecutionContext executionContext,
      IEnumerable<TItem> items,
      Func<List<TItem>, TExecutionContext, Task> processChunk)
    {
      const int maxBatchSize = 100;

      var enumerator = items.GetEnumerator();
      var chunkItems = new List<TItem>();

      for (var itemsAvaliable = true; itemsAvaliable;)
      {
        chunkItems.Clear();
        itemsAvaliable = FillChunkList(enumerator, maxBatchSize, chunkItems);
        if (chunkItems.Count > 0)
          await processChunk(chunkItems, executionContext);
      }

      return executionContext;
    }

    public static TExecutionContext Execute<TItem, TExecutionContext>(
      TExecutionContext executionContext,
      IEnumerable<TItem> items,
      Action<List<TItem>, TExecutionContext> processChunk)
    {
      return ExecuteAsync(executionContext, items, (chunk, context) =>
      {
        processChunk(chunk, context);
        return Task.FromResult(0);
      }).Result;
    }

    private static bool FillChunkList<T> (IEnumerator<T> enumerator, int batchSize, List<T> list)
    {
      for (int i = 0; i < batchSize; i++)
      {
        if (!enumerator.MoveNext ())
          return false;

        list.Add (enumerator.Current);
      }

      return true;
    }
  }
}
