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

namespace GenSync.Synchronization
{
    /// <summary>
    /// Creates a synchronization interceptor
    /// In contrast to the synchronization context, which represents just user defined data, which is opaque for the Synchronizer and just passed through the
    /// whole callchain, the <see cref="ISynchronizationInterceptor"/> is an interface, which is called by the synchronizer at defined steps and
    /// which can be used to customize the synchronization process.
    /// </summary>
    public interface ISynchronizationInterceptorFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
    {
        /// <summary>
        /// Is called at the beginning of every sync run, to create an interceptor for the ongoing sync run
        /// </summary>
        /// <returns></returns>
        ISynchronizationInterceptor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> Create();
    }
}