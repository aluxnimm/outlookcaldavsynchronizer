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
using GenSync.Synchronization;

namespace GenSync.EntityRepositories
{
  public class BatchEntityRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext> :
      IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext>
  {
    private readonly IWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _inner;
    private readonly IExceptionHandlingStrategy _exceptionHandlingStrategy;

    public BatchEntityRepositoryAdapter (
      IWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> inner, 
      IExceptionHandlingStrategy exceptionHandlingStrategy)
    {
      if (inner == null)
        throw new ArgumentNullException (nameof (inner));
      if (exceptionHandlingStrategy == null) throw new ArgumentNullException(nameof(exceptionHandlingStrategy));

      _inner = inner;
      _exceptionHandlingStrategy = exceptionHandlingStrategy;
    }

    public async Task PerformOperations (
        IReadOnlyList<ICreateJob<TEntityId, TEntityVersion, TEntity>> createJobs,
        IReadOnlyList<IUpdateJob<TEntityId, TEntityVersion, TEntity>> updateJobs,
        IReadOnlyList<IDeleteJob<TEntityId, TEntityVersion>> deleteJobs,
        IProgressLogger progressLogger,
        TContext context)
    {
      foreach (var job in createJobs)
      {
        try
        {
          var result = await _inner.Create (job.InitializeEntity, context);
          job.NotifyOperationSuceeded (result);
        }
        catch (Exception x)
        {
          if (_exceptionHandlingStrategy.DoesGracefullyAbortSynchronization(x))
            throw;
          else
            job.NotifyOperationFailed (x);
        }
        progressLogger.Increase();
      }

      foreach (var job in updateJobs)
      {
        try
        {
          var result = await _inner.TryUpdate (job.EntityId, job.Version, job.EntityToUpdate, job.UpdateEntity , context, job.Logger);
          if (result != null)
            job.NotifyOperationSuceeded (result);
          else
            job.NotifyEntityNotFound();
        }
        catch (Exception x)
        {
          if (_exceptionHandlingStrategy.DoesGracefullyAbortSynchronization(x))
            throw;
          else
            job.NotifyOperationFailed (x);
        }
        progressLogger.Increase();
      }

      foreach (var job in deleteJobs)
      {
        try
        {
          if (await _inner.TryDelete (job.EntityId, job.Version, context, job.Logger))
            job.NotifyOperationSuceeded();
          else
            job.NotifyEntityNotFound();
        }
        catch (Exception x)
        {
          if (_exceptionHandlingStrategy.DoesGracefullyAbortSynchronization(x))
            throw;
          else
            job.NotifyOperationFailed (x);
        }
        progressLogger.Increase();
      }
    }
  }

  public static class BatchEntityRepositoryAdapter
  {
    public static IBatchWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion, int>
      Create<TEntity, TEntityId, TEntityVersion>(
        IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion, int> inner,
        IExceptionHandlingStrategy exceptionHandlingStrategy)
    {
      return new BatchEntityRepositoryAdapter<TEntity, TEntityId, TEntityVersion, int>(inner, exceptionHandlingStrategy);
    }

    public static IBatchWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion, TContext>
      Create<TEntity, TEntityId, TEntityVersion, TContext>(
        IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion, TContext> inner,
        IExceptionHandlingStrategy exceptionHandlingStrategy)
    {
      return new BatchEntityRepositoryAdapter<TEntity, TEntityId, TEntityVersion, TContext>(inner, exceptionHandlingStrategy);
    }
  }
}