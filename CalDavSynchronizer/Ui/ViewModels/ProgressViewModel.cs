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
using GenSync.ProgressReport;

namespace CalDavSynchronizer.Ui.ViewModels
{
  public class ProgressViewModel : ModelBase, IProgressUi
  {
    private int _value;
    private int _maximum;
    private string _message;
    private string _subMessage;

    public static ProgressViewModel DesingInstance => new ProgressViewModel() { Maximum = 300, Value = 100, Message = "One third", SubMessage ="Submessage" };

    public event EventHandler CloseRequested;

    public int Value
    {
      get => _value;
      set => CheckedPropertyChange(ref _value, value);
    }

    public int Maximum
    {
      get => _maximum;
      set => CheckedPropertyChange(ref _maximum, value);
    }

    public string Message
    {
      get => _message;
      set => CheckedPropertyChange(ref _message, value);
    }

    public void IncrementValue()
    {
      Value += 1;
      System.Windows.Forms.Application.DoEvents();
    }

    public string SubMessage
    {
      get => _subMessage;
      set => CheckedPropertyChange(ref _subMessage, value);
    }

    public void SetMessage(string message)
    {
      Message = message;
      System.Windows.Forms.Application.DoEvents();
    }

    public void SetSubMessage(string message)
    {
      SubMessage = message;
      System.Windows.Forms.Application.DoEvents();
    }

    public void SetMaximun(int value)
    {
      Maximum = value;
    }

    void IDisposable.Dispose()
    {
      CloseRequested?.Invoke(this, EventArgs.Empty);
    }
  }
}
