// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenSync.Utilities
{
  public class ChunkedExecutor : IChunkedExecutor
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
