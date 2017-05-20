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
using CalDavSynchronizer.Implementation.ComWrappers;
using Microsoft.Office.Interop.Outlook;
using Action = System.Action;

namespace CalDavSynchronizer.IntegrationTests.TestBase.ComWrappers
{
  class TestTaskItemWrapper : TestComWrapperBase<TestTaskItemWrapper>, ITaskItemWrapper
  {
    private readonly ITaskItemWrapper _inner;

    public TestTaskItemWrapper(Action<TestTaskItemWrapper> onDisposed, ITaskItemWrapper inner) : base(onDisposed)
    {
      _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    protected override void DisposeOverride()
    {
      _inner.Dispose();
    }

    protected override TestTaskItemWrapper This()
    {
      return this;
    }

    public TaskItem Inner => _inner.Inner;

    public void SaveAndReload()
    {
      _inner.SaveAndReload();
    }

    public void Replace(TaskItem inner)
    {
      _inner.Replace(inner);
    }
  }
}