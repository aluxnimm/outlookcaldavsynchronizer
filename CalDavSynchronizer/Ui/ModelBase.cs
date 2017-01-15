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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.Ui
{
  public class ModelBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly Dictionary<INotifyPropertyChanged, Dictionary<string, List<string>>> _propertyPropagationsBySource = new Dictionary<INotifyPropertyChanged, Dictionary<string, List<string>>>();
    private readonly Dictionary<INotifyPropertyChanged, Dictionary<string, List<Action>>> _propertyChangeHandlersBySource = new Dictionary<INotifyPropertyChanged, Dictionary<string, List<Action>>>();

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool CheckedPropertyChange<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
    {
      if (!Equals(backingField, newValue))
      {
        backingField = newValue;
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(propertyName);
        return true;
      }

      return false;
    }

    protected void RegisterPropertyChangePropagation(INotifyPropertyChanged source, string sourceProperty, string targetProperty)
    {
      _propertyPropagationsBySource.GetOrAdd(
        source,
        () =>
        {
          source.PropertyChanged += Source_PropertyChanged;
          return new Dictionary<string, List<string>>();
        }
      ).GetOrAdd(sourceProperty).Add(targetProperty);
    }

    protected void RegisterPropertyChangeHandler(INotifyPropertyChanged source, string sourceProperty, Action handler)
    {
      _propertyChangeHandlersBySource.GetOrAdd(
        source,
        () =>
        {
          source.PropertyChanged += Source_PropertyChanged;
          return new Dictionary<string, List<Action>>();
        }
      ).GetOrAdd(sourceProperty).Add(handler);

    }

    private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      Dictionary<string, List<string>> propertyPropagationsForSender;
      if (_propertyPropagationsBySource.TryGetValue((INotifyPropertyChanged) sender, out propertyPropagationsForSender))
      {
        List<string> targetProperties;
        if (propertyPropagationsForSender.TryGetValue(e.PropertyName, out targetProperties))
        {
          foreach (var targetProperty in targetProperties)
            OnPropertyChanged(targetProperty);
        }
      }

      Dictionary<string, List<Action>> propertyChangedHandlersForSender;
      if (_propertyChangeHandlersBySource.TryGetValue((INotifyPropertyChanged) sender, out propertyChangedHandlersForSender))
      {
        List<Action> changeHandlers;
        if (propertyChangedHandlersForSender.TryGetValue(e.PropertyName, out changeHandlers))
        {
          foreach (var changeHandler in changeHandlers)
            changeHandler();
        }
      }
    }
  }
}