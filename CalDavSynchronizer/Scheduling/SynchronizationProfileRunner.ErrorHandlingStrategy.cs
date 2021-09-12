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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Reports;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Utilities;
using CalDavSynchronizer.Ui.ConnectionTests;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.Synchronization;
using log4net;

namespace CalDavSynchronizer.Scheduling
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class is NOT threadsafe, but is safe regarding multiple entries of the same thread ( main thread)
    /// which will is a common case when using async methods
    /// </remarks>
    public partial class SynchronizationProfileRunner
    {
        struct ErrorHandlingStrategy
        {
            private readonly int _maxSucessiveWarnings;

            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly ProfileData _profile;

            private DateTime? _postponeUntil;
            private int _successiveWarningsCount;
            private bool _currentSyncRunCausedWarning;
            private bool _currentRunWasManuallyTriggered;

            public ErrorHandlingStrategy(ProfileData profile, IDateTimeProvider dateTimeProvider, int maxSucessiveWarnings)
            {
                _profile = profile;
                _dateTimeProvider = dateTimeProvider;
                _maxSucessiveWarnings = maxSucessiveWarnings;

                _postponeUntil = null;
                _successiveWarningsCount = 0;
                _currentSyncRunCausedWarning = false;
                _currentRunWasManuallyTriggered = false;
            }

            public bool ShouldPostponeSyncRun()
            {
                if (_postponeUntil >= _dateTimeProvider.Now)
                {
                    s_logger.Info($"Profile '{_profile.ProfileName}' will not run, since it is postponed until '{_postponeUntil}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void NotifySyncRunStarting(bool wasManuallyTriggered)
            {
                _currentRunWasManuallyTriggered = wasManuallyTriggered;
                _currentSyncRunCausedWarning = false;
                _postponeUntil = null;
            }

            public void NotifySyncRunFinished()
            {
                if (!_currentSyncRunCausedWarning)
                    _successiveWarningsCount = 0;
            }

            public void HandleException(Exception exception, ISynchronizationLogger logger)
            {
                PostponeIfRequired(exception);

                if (!_currentRunWasManuallyTriggered && IsWarning(exception))
                {
                    _successiveWarningsCount++;
                    if (_successiveWarningsCount > _maxSucessiveWarnings)
                    {
                        _successiveWarningsCount = 0;
                        LogError(exception, logger);
                    }
                    else
                    {
                        _currentSyncRunCausedWarning = true;
                        logger.LogAbortedDueToWarning(exception);
                        s_logger.Warn(exception);
                    }
                }
                else
                {
                    LogError(exception, logger);
                }
            }

            private static void LogError(Exception exception, ISynchronizationLogger logger)
            {
                logger.LogAbortedDueToError(exception);
                ExceptionHandler.Instance.LogException(exception, s_logger);
            }

            void PostponeIfRequired(Exception exception)
            {
                var overloadException = exception as WebRepositoryOverloadException;
                if (overloadException != null)
                {
                    _postponeUntil = overloadException.RetryAfter;
                    if (_postponeUntil.HasValue)
                        s_logger.Warn($"Postponing following runs until '{_postponeUntil}'.");
                }
            }

            bool IsWarning(Exception x)
            {
                return
                    x is WebRepositoryOverloadException ||
                    x is TaskCanceledException ||
                    x.IsTimeoutException();
            }
        }
    }
}