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
using System.Linq;
using System.Reflection;
using log4net;

namespace GenSync.ProgressReport
{
    public class TotalProgressLogger : ITotalProgressLogger
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
        private readonly IExceptionLogger _exceptionLogger;
        private int _currentChunk = -1;

        private readonly IProgressUi _progressUi;

        public TotalProgressLogger(IProgressUiFactory uiFactory, IExceptionLogger exceptionLogger, int chunkCount)
        {
            _exceptionLogger = exceptionLogger;
            _progressUi = uiFactory.Create(chunkCount * 3);
        }

        public void Dispose()
        {
            try
            {
                _progressUi.Dispose();
            }
            catch (Exception x)
            {
                _exceptionLogger.LogException(x, s_logger);
            }
        }

        public void NotifyWork(int totalEntitiesBeingLoaded, int chunkCount)
        {
        }

        public IChunkProgressLogger StartChunk()
        {
            _progressUi.SetMessage($"Processing chunk #{++_currentChunk + 1}");
            return new ChunkProgressLogger(_progressUi, _exceptionLogger);
        }
    }
}