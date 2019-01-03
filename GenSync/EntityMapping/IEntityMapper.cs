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
using System.Threading.Tasks;
using GenSync.Logging;

namespace GenSync.EntityMapping
{
  /// <summary>
  /// Maps a entity from one physical representation into another and vice versa
  /// </summary>
  public interface IEntityMapper<T1, T2, TContext>
  {
    Task<T2> Map1To2 (T1 source, T2 target, IEntitySynchronizationLogger logger, TContext context);
    Task<T1> Map2To1 (T2 source, T1 target, IEntitySynchronizationLogger logger, TContext context);
  }
}