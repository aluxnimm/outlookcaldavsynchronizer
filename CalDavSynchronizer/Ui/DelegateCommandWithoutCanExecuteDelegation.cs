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
using System.Reflection;
using System.Windows.Input;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Ui
{
  public class DelegateCommandWithoutCanExecuteDelegation : ICommand
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Action<object> _execute;
    private bool _canExecute = true;

    public event EventHandler CanExecuteChanged;

   

    public DelegateCommandWithoutCanExecuteDelegation (Action<object> execute)
    {
      _execute = execute;
    }

    public bool CanExecute (object parameter)
    {
      return _canExecute;
    }

    public void Execute (object parameter)
    {
      try
      {
        _execute (parameter);
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    public void SetCanExecute (bool value)
    {
      if (_canExecute != value)
      {
        _canExecute = value;
        CanExecuteChanged?.Invoke (this, EventArgs.Empty);
      }
    }
  }
}