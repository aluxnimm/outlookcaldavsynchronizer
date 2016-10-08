using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Utilities
{
  public class ChunkedExecutor
  {
    private readonly int _maxChunkSize;

    public ChunkedExecutor(int maxChunkSize)
    {
      _maxChunkSize = maxChunkSize;
    }

    public async Task<TExecutionContext> ExecuteAsync<TItem, TExecutionContext>(
      TExecutionContext executionContext,
      IEnumerable<TItem> items,
      Func<List<TItem>, TExecutionContext, Task> processChunk)
    {
      var enumerator = items.GetEnumerator();
      var chunkItems = new List<TItem>();

      for (var itemsAvaliable = true; itemsAvaliable;)
      {
        chunkItems.Clear();
        itemsAvaliable = FillChunkList(enumerator, chunkItems);
        if (chunkItems.Count > 0)
          await processChunk(chunkItems, executionContext);
      }

      return executionContext;
    }

    public TExecutionContext Execute<TItem, TExecutionContext>(
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

    private bool FillChunkList<T> (IEnumerator<T> enumerator, List<T> list)
    {
      for (int i = 0; i < _maxChunkSize; i++)
      {
        if (!enumerator.MoveNext ())
          return false;

        list.Add (enumerator.Current);
      }

      return true;
    }
  }
}
