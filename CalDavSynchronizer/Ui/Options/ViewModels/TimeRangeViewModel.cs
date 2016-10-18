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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class TimeRangeViewModel : ViewModelBase, IOptionsSection
  {
    private int _daysToSynchronizeInTheFuture;
    private int _daysToSynchronizeInThePast;
    private bool _useSynchronizationTimeRange;
    
    public bool UseSynchronizationTimeRange
    {
      get { return _useSynchronizationTimeRange; }
      set
      {
        CheckedPropertyChange (ref _useSynchronizationTimeRange, value);
      }
    }

    public int DaysToSynchronizeInThePast
    {
      get { return _daysToSynchronizeInThePast; }
      set
      {
        CheckedPropertyChange (ref _daysToSynchronizeInThePast, value);
      }
    }

    public int DaysToSynchronizeInTheFuture
    {
      get { return _daysToSynchronizeInTheFuture; }
      set
      {
        CheckedPropertyChange (ref _daysToSynchronizeInTheFuture, value);
      }
    }

    
    public static TimeRangeViewModel DesignInstance { get; } = new TimeRangeViewModel
                                                                  {
                                                                       UseSynchronizationTimeRange = true,
                                                                        DaysToSynchronizeInTheFuture = 11,
                                                                         DaysToSynchronizeInThePast = 22
                                                                  };

    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      UseSynchronizationTimeRange = !options.IgnoreSynchronizationTimeRange;
      DaysToSynchronizeInThePast = options.DaysToSynchronizeInThePast;
      DaysToSynchronizeInTheFuture = options.DaysToSynchronizeInTheFuture;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.IgnoreSynchronizationTimeRange = !_useSynchronizationTimeRange;
      options.DaysToSynchronizeInThePast = _daysToSynchronizeInThePast;
      options.DaysToSynchronizeInTheFuture = _daysToSynchronizeInTheFuture;
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }
  }
}