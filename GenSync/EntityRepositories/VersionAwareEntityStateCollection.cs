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
using log4net;

namespace GenSync.EntityRepositories
{
    class VersionAwareEntityStateCollection<TEntityId, TEntityVersion> : IEntityStateCollection<TEntityId, TEntityVersion>
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<TEntityId, TEntityVersion> _repositoryVersionById;
        private readonly IEqualityComparer<TEntityVersion> _versionComparer;

        private VersionAwareEntityStateCollection(Dictionary<TEntityId, TEntityVersion> repositoryVersionById, IEqualityComparer<TEntityVersion> versionComparer)
        {
            _repositoryVersionById = repositoryVersionById ?? throw new ArgumentNullException(nameof(repositoryVersionById));
            _versionComparer = versionComparer ?? throw new ArgumentNullException(nameof(versionComparer));
        }

        public static IEntityStateCollection<TEntityId, TEntityVersion> Create(IEnumerable<EntityVersion<TEntityId, TEntityVersion>> repositoryVersions, IEqualityComparer<TEntityId> idComparer, IEqualityComparer<TEntityVersion> versionComparer)
        {
            var repositoryVersionById = CreateDictionary(repositoryVersions, idComparer);
            return new VersionAwareEntityStateCollection<TEntityId, TEntityVersion>(repositoryVersionById, versionComparer);
        }

        private static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(IEnumerable<EntityVersion<TKey, TValue>> tuples, IEqualityComparer<TKey> equalityComparer)
        {
            var dictionary = new Dictionary<TKey, TValue>(equalityComparer);

            foreach (var tuple in tuples)
            {
                if (!dictionary.ContainsKey(tuple.Id))
                    dictionary.Add(tuple.Id, tuple.Version);
                else
                    s_logger.WarnFormat("EntitiyVersion '{0}' was contained multiple times in repository response. Ignoring redundant entity", tuple.Id);
            }

            return dictionary;
        }

        public (EntityState State, TEntityVersion RepositoryVersion) RemoveState(TEntityId id, TEntityVersion knownVersion)
        {
            if (_repositoryVersionById.TryGetValue(id, out var repositoryVersion))
            {
                _repositoryVersionById.Remove(id);
                if (_versionComparer.Equals(knownVersion, repositoryVersion))
                    return (EntityState.Unchanged, repositoryVersion);
                else
                    return (EntityState.ChangedOrAdded, repositoryVersion);
            }
            else
            {
                return (EntityState.Deleted, repositoryVersion);
            }
        }

        public Dictionary<TEntityId, TEntityVersion> DisposeAndGetLeftovers() => _repositoryVersionById;
    }
}