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
using System.Text;
using CalDavSynchronizer.Ui.Options.Models;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class TimeRangeViewModel : ModelBase, IOptionsSection
  {
    private readonly OptionsModel _model;

    public TimeRangeViewModel(OptionsModel model, IViewOptions viewOptions)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      _model = model;
      ViewOptions = viewOptions;

      RegisterPropertyChangePropagation(_model, nameof(_model.UseSynchronizationTimeRange), nameof(UseSynchronizationTimeRange));
      RegisterPropertyChangePropagation(_model, nameof(_model.DaysToSynchronizeInThePast), nameof(DaysToSynchronizeInThePast));
      RegisterPropertyChangePropagation(_model, nameof(_model.DaysToSynchronizeInTheFuture), nameof(DaysToSynchronizeInTheFuture));
    }

    public bool UseSynchronizationTimeRange
    {
      get { return _model.UseSynchronizationTimeRange; }
      set { _model.UseSynchronizationTimeRange = value; }
    }

    public int DaysToSynchronizeInThePast
    {
      get { return _model.DaysToSynchronizeInThePast; }
      set { _model.DaysToSynchronizeInThePast = value; }
    }

    public int DaysToSynchronizeInTheFuture
    {
      get { return _model.DaysToSynchronizeInTheFuture; }
      set { _model.DaysToSynchronizeInTheFuture = value; }
    }

    public IViewOptions ViewOptions { get; }


    //public static TimeRangeViewModel DesignInstance { get; } = new TimeRangeViewModel
    //                                                              {
    //                                                                   UseSynchronizationTimeRange = true,
    //                                                                    DaysToSynchronizeInTheFuture = 11,
    //                                                                     DaysToSynchronizeInThePast = 22
    //                                                              };

   
  }
}