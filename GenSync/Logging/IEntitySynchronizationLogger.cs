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

namespace GenSync.Logging
{
  public interface IEntitySynchronizationLogger : IDisposable
  {
    void LogError(string message);
    void LogError(string message, Exception exception);
    void LogWarning(string warning);
    void LogWarning(string warning, Exception exception);
  }

  public interface IEntitySynchronizationLogger<in TAtypeEntityId, in TAtypeEntity, in TBtypeEntityId, in TBtypeEntity> : IEntitySynchronizationLogger
  {
    void LogA(TAtypeEntity entity);
    void LogB(TBtypeEntity entity);
    void SetAId(TAtypeEntityId id);
    void SetBId(TBtypeEntityId id);
    void LogAbortedDueToError(Exception exception);
    void LogAbortedDueToError(string errorMessage);
  }
}