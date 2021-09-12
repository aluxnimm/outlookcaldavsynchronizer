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

namespace GenSync.Logging
{
    class LoadEntityLogger : ILoadEntityLogger, IGetVersionsLogger
    {
        private readonly List<LoadError> _loadErrors;
        private readonly object _loadErrorsLock;
        private readonly bool _isARepository;

        public LoadEntityLogger(List<LoadError> loadErrors, object loadErrorsLock, bool isARepository)
        {
            _loadErrors = loadErrors;
            _loadErrorsLock = loadErrorsLock;
            _isARepository = isARepository;
        }

        public void LogSkipLoadBecauseOfError(object entityId, Exception exception)
        {
            lock (_loadErrorsLock)
            {
                _loadErrors.Add(new LoadError
                {
                    EntityId = entityId.ToString(),
                    Error = exception.ToString(),
                    IsAEntity = _isARepository
                });
            }
        }

        public void LogWarning(object entityId, string message)
        {
            lock (_loadErrorsLock)
            {
                _loadErrors.Add(new LoadError
                {
                    EntityId = entityId.ToString(),
                    Error = message,
                    IsAEntity = _isARepository,
                    IsWarning = true
                });
            }
        }

        public void LogError(object entityId, string message)
        {
            lock (_loadErrorsLock)
            {
                _loadErrors.Add(new LoadError
                {
                    EntityId = entityId.ToString(),
                    Error = message,
                    IsAEntity = _isARepository,
                    IsWarning = false
                });
            }
        }
    }
}