﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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

using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalDavSynchronizer.Scheduling
{
    public class Scheduler
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private readonly Timer _synchronizationTimer = new Timer();

        private Dictionary<Guid, SynchronizationProfileRunner> _runnersById = new Dictionary<Guid, SynchronizationProfileRunner>();
        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(30);
        private readonly ISynchronizerFactory _synchronizerFactory;
        private readonly ISynchronizationReportSink _reportSink;
        private readonly IFolderChangeWatcherFactory _folderChangeWatcherFactory;
        private readonly IUiService _uiService;
        private readonly Action _ensureSynchronizationContext;
        private readonly ISynchronizationRunLogger _runLogger;

        public Scheduler(
            ISynchronizerFactory synchronizerFactory,
            ISynchronizationReportSink reportSink,
            Action ensureSynchronizationContext,
            IFolderChangeWatcherFactory folderChangeWatcherFactory,
            ISynchronizationRunLogger runLogger,
            IUiService uiService)
        {
            if (synchronizerFactory == null)
                throw new ArgumentNullException(nameof(synchronizerFactory));
            if (ensureSynchronizationContext == null)
                throw new ArgumentNullException(nameof(ensureSynchronizationContext));
            if (folderChangeWatcherFactory == null)
                throw new ArgumentNullException(nameof(folderChangeWatcherFactory));
            if (runLogger == null)
                throw new ArgumentNullException(nameof(runLogger));
            if (reportSink == null)
                throw new ArgumentNullException(nameof(reportSink));

            _reportSink = reportSink;
            _synchronizerFactory = synchronizerFactory;
            _ensureSynchronizationContext = ensureSynchronizationContext;
            _folderChangeWatcherFactory = folderChangeWatcherFactory;
            _runLogger = runLogger;
            _synchronizationTimer.Tick += SynchronizationTimer_Tick;
            _synchronizationTimer.Interval = (int) _timerInterval.TotalMilliseconds;
            _uiService = uiService;
        }

        public void Start()
        {
            _synchronizationTimer.Start();
        }


        private void SynchronizationTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _ensureSynchronizationContext();
                RunTimeTriggeredSynchronization();
            }
            catch (Exception x)
            {
                s_logger.Error(null, x);
            }
        }

        private async void RunTimeTriggeredSynchronization()
        {
            try
            {
                _synchronizationTimer.Stop();

                using (_runLogger.LogStartSynchronizationRun())
                {
                    foreach (var worker in _runnersById.Values)
                    {
                        await worker.RunAndRescheduleNoThrow(false);
                    }
                }

                _synchronizationTimer.Start();
            }
            catch (Exception x)
            {
                s_logger.Error(null, x);
            }
        }

        public async Task SetOptions(Options[] options, GeneralOptions generalOptions)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (generalOptions == null)
                throw new ArgumentNullException(nameof(generalOptions));

            Dictionary<Guid, SynchronizationProfileRunner> workersById = new Dictionary<Guid, SynchronizationProfileRunner>();
            foreach (var option in options)
            {
                try
                {
                    SynchronizationProfileRunner profileRunner;
                    if (!_runnersById.TryGetValue(option.Id, out profileRunner))
                    {
                        profileRunner = new SynchronizationProfileRunner(
                            _synchronizerFactory,
                            _reportSink,
                            _folderChangeWatcherFactory,
                            _ensureSynchronizationContext,
                            _runLogger,
                            DateTimeProvider.Instance,
                            option.Id);
                    }

                    await profileRunner.UpdateOptions(option, generalOptions);
                    workersById.Add(option.Id, profileRunner);
                }
                catch (Exception x)
                {
                    ExceptionHandler.Instance.LogException(x, s_logger);
                }
            }

            _runnersById = workersById;
        }

        public async Task RunNow(IProgress<float> progressBar = null)
        {
            
            using (_runLogger.LogStartSynchronizationRun())
            {
                float progress = 0, step = 0;
                if(_runnersById.Count > 0)
                    step = 1f / _runnersById.Count;

                foreach (var worker in _runnersById.Values)
                {
                    await worker.RunAndRescheduleNoThrow(true);

                    progress += step;
                    progressBar?.Report(progress);
                }
            }
        }
    }
}