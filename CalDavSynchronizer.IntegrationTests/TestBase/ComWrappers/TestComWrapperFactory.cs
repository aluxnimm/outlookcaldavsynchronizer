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

using System.Collections.Generic;
using CalDavSynchronizer.Implementation.ComWrappers;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase.ComWrappers
{
  class TestComWrapperFactory : IComWrapperFactory
  {
    private readonly InstanceCounter<IAppointmentItemWrapper> _appointment;
    private readonly InstanceCounter<IContactItemWrapper> _contactItem;
    private readonly InstanceCounter<ITaskItemWrapper> _taskItem;
    private readonly InstanceCounter<IDistListItemWrapper> _distList;

    public TestComWrapperFactory(int? maximumOpenItemsPerType)
    {
      _appointment = new InstanceCounter<IAppointmentItemWrapper>(maximumOpenItemsPerType);
      _contactItem = new InstanceCounter<IContactItemWrapper>(maximumOpenItemsPerType);
      _taskItem = new InstanceCounter<ITaskItemWrapper>(maximumOpenItemsPerType);
      _distList = new InstanceCounter<IDistListItemWrapper>(maximumOpenItemsPerType);
    }

    public void AssertNoInstancesOpen()
    {
      _appointment.AssertNoInstancesOpen();
      _contactItem.AssertNoInstancesOpen();
      _taskItem.AssertNoInstancesOpen();
      _distList.AssertNoInstancesOpen();
    }

    public IAppointmentItemWrapper Create(AppointmentItem inner, LoadAppointmentItemDelegate load)
    {
      var wrapper = new TestAppointmentItemWrapper(_appointment.Release, new AppointmentItemWrapper(inner, load));
      _appointment.Open(wrapper);
      return wrapper;
    }

    public IContactItemWrapper Create(ContactItem inner, LoadContactItemDelegate load)
    {
      var wrapper = new TestContactItemWrapper(_contactItem.Release, new ContactItemWrapper(inner, load));
      _contactItem.Open(wrapper);
      return wrapper;
    }

    public ITaskItemWrapper Create(TaskItem inner, LoadTaskItemDelegate load)
    {
      var wrapper = new TestTaskItemWrapper(_taskItem.Release, new TaskItemWrapper(inner, load));
      _taskItem.Open(wrapper);
      return wrapper;
    }

    public IDistListItemWrapper Create(DistListItem inner)
    {
      var wrapper = new TestDistListItemWrapper(_distList.Release, new DistListItemWrapper(inner));
      _distList.Open(wrapper);
      return wrapper;
    }

    class InstanceCounter<T>
    {
      private readonly HashSet<T> _instances = new HashSet<T>();
      private readonly int? _maximumOpenItemsPerType;

      public InstanceCounter(int? maximumOpenItemsPerType)
      {
        _maximumOpenItemsPerType = maximumOpenItemsPerType;
      }

      public void Open(T instance)
      {
        if(!_instances.Add(instance))
          Assert.Fail($"Internal test error. Cannot open an item twice");

        if (_instances.Count > _maximumOpenItemsPerType)
          Assert.Fail($"Threshold of allowed open items has been exceeded");
      }

      public void Release(T instance)
      {
        if(!_instances.Remove(instance))
          Assert.Fail($"Internal test error. Cannot release an item twice");
      }

      public void AssertNoInstancesOpen()
      {
        Assert.That(_instances.Count, Is.EqualTo(0));
      }
    }

  }
}
