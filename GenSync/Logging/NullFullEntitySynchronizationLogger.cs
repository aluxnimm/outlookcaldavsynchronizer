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

using System;

namespace GenSync.Logging
{
    public class NullFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> : IFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>
    {
        public static readonly IFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> Instance = new NullFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>();

        private NullFullEntitySynchronizationLogger()
        {
        }

        public void SetAId(TAtypeEntityId aid)
        {
        }

        public void SetBId(TBtypeEntityId bid)
        {
        }

        public void LogAbortedDueToError(Exception exception)
        {
        }

        public void LogAbortedDueToError(string errorMessage)
        {
        }

        public void LogError(string message)
        {
        }

        public void LogError(string message, Exception exception)
        {
        }

        public void LogWarning(string warning)
        {
        }

        public void LogWarning(string warning, Exception exception)
        {
        }

        public void Dispose()
        {
        }

        public void LogA(TAtypeEntity entity)
        {
        }

        public void LogB(TBtypeEntity entity)
        {
        }

        public bool HasErrorsOrWarnings => false;

        public EntitySynchronizationReport GetReport()
        {
            return new EntitySynchronizationReport();
        }
    }
}