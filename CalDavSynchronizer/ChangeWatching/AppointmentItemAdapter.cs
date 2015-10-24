// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.ChangeWatching
{
  internal class AppointmentItemAdapter : IOutlookItem
  {
    private AppointmentItem _appointment;
    private Inspector _inspector;

    public event EventHandler Saved;
    public event EventHandler Closed;

    public AppointmentItemAdapter (Inspector inspector, AppointmentItem appointment)
    {
      _appointment = appointment;
      _inspector = inspector;

      _appointment.AfterWrite += Appointment_AfterWrite;
      ((InspectorEvents_10_Event) _inspector).Close += AppointmentItemWrapper_Close;
    }

    private void Appointment_AfterWrite ()
    {
      OnSaved();
    }

    private void AppointmentItemWrapper_Close ()
    {
      OnClosed();
    }

    protected virtual void OnSaved ()
    {
      var handler = Saved;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    protected virtual void OnClosed ()
    {
      var handler = Closed;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public string EntryId
    {
      get { return _appointment.EntryID; }
    }

    public Tuple<string, string> FolderEntryIdAndStoreIdOrNull
    {
      get
      {
        var folder = _appointment.Parent as MAPIFolder;
        return folder != null ? Tuple.Create (folder.EntryID, folder.StoreID) : null;
      }
    }

    public void Dispose ()
    {
      _appointment.AfterWrite -= Appointment_AfterWrite;
      ((InspectorEvents_10_Event) _inspector).Close -= AppointmentItemWrapper_Close;

      _appointment = null;
      _inspector = null;
    }
  }
}