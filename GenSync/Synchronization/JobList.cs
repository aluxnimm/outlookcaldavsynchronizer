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
using GenSync.EntityRepositories;

namespace GenSync.Synchronization
{
  class JobList<TEntity, TEntityId, TEntityVersion> : IJobList<TEntity, TEntityId, TEntityVersion>
  {
    private readonly List<ICreateJob<TEntity, TEntityId, TEntityVersion>> _createJobs = new List<ICreateJob<TEntity, TEntityId, TEntityVersion>>();
    private readonly List<IUpdateJob<TEntity, TEntityId, TEntityVersion>> _updateJobs = new List<IUpdateJob<TEntity, TEntityId, TEntityVersion>>();
    private readonly List<IDeleteJob<TEntityId, TEntityVersion>> _deleteJobs = new List<IDeleteJob<TEntityId, TEntityVersion>>();

    public List<ICreateJob<TEntity, TEntityId, TEntityVersion>> CreateJobs => _createJobs;
    public List<IUpdateJob<TEntity, TEntityId, TEntityVersion>> UpdateJobs => _updateJobs;
    public List<IDeleteJob<TEntityId, TEntityVersion>> DeleteJobs => _deleteJobs;

    public void AddCreateJob (ICreateJob<TEntity, TEntityId, TEntityVersion> job)
    {
      _createJobs.Add (job);
    }

    public void AddUpdateJob (IUpdateJob<TEntity, TEntityId, TEntityVersion> job)
    {
      _updateJobs.Add (job);
    }

    public void AddDeleteJob (IDeleteJob<TEntityId, TEntityVersion> job)
    {
      _deleteJobs.Add (job);
    }

    public int TotalJobCount => _createJobs.Count + _updateJobs.Count + _deleteJobs.Count;
  }
}