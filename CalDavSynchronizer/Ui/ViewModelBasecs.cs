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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace CalDavSynchronizer.Ui
{
  public class ViewModelBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }


    protected void CheckedPropertyChange<T> (ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
    {
      if (!Equals (backingField, newValue))
      {
        backingField = newValue;
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (propertyName);
      }
    }
  }
}