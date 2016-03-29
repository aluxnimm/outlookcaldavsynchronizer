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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.ProgressReport;

namespace GenSync.EntityRepositories
{
  public class BatchEntityRepositoryAdapter<TEntity, TEntityId, TEntityVersion, TContext> :
      IBatchWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion, TContext>
  {
    private readonly IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion> _inner;

    public BatchEntityRepositoryAdapter (IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion> inner)
    {
      if (inner == null)
        throw new ArgumentNullException (nameof (inner));

      _inner = inner;
    }

    public async Task PerformOperations (
        IEnumerable<ICreateJob<TEntity, TEntityId, TEntityVersion>> createJobs,
        IEnumerable<IUpdateJob<TEntity, TEntityId, TEntityVersion>> updateJobs,
        IEnumerable<IDeleteJob<TEntityId, TEntityVersion>> deleteJobs,
        IProgressLogger progressLogger,
        TContext context)
    {
      foreach (var job in createJobs)
      {
        try
        {
          var result = await _inner.Create (job.InitializeEntity);
          job.NotifyOperationSuceeded (result);
        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
        progressLogger.Increase();
      }

      foreach (var job in updateJobs)
      {
        try
        {
          var result = await _inner.Update (job.EntityId, job.Version, job.EntityToUpdate, job.UpdateEntity);
          job.NotifyOperationSuceeded (result);
        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
        progressLogger.Increase();
      }

      foreach (var job in deleteJobs)
      {
        try
        {
          await _inner.Delete (job.EntityId, job.Version);
          job.NotifyOperationSuceeded();
        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
        progressLogger.Increase();
      }
    }
  }

  public static class BatchEntityRepositoryAdapter
  {
    public static IBatchWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion, int>
        Create<TEntity, TEntityId, TEntityVersion> (IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion> inner)
    {
      return new BatchEntityRepositoryAdapter<TEntity, TEntityId, TEntityVersion, int> (inner);
    }
  }
}