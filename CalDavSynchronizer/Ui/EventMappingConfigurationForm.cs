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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui
{
  public partial class EventMappingConfigurationForm : Form, IConfigurationForm<EventMappingConfiguration>
  {
    private readonly IList<Item<ReminderMapping>> _availableReminderMappings = new List<Item<ReminderMapping>> ()
                                                                               {
                                                                                   new Item<ReminderMapping> (ReminderMapping.@true, "Yes"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.@false, "No"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders"),
                                                                               };

    private readonly Func<ICalDavDataAccess> _calDavDataAccessFactory;

    public EventMappingConfigurationForm (Func<ICalDavDataAccess> calDavDataAccessFactory)
    {
      InitializeComponent ();
      Item.BindComboBox (_mapReminderComboBox, _availableReminderMappings);

      _calDavDataAccessFactory = calDavDataAccessFactory;

      _categoryColorPicker.AddCategoryColors ();
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public bool Display ()
    {
      return ShowDialog () == DialogResult.OK;
    }

    public EventMappingConfiguration Options
    {
      get
      {
        return new EventMappingConfiguration
        {
          MapAttendees = _mapAttendeesCheckBox.Checked,
          MapBody = _mapBodyCheckBox.Checked,
          MapReminder = (ReminderMapping) _mapReminderComboBox.SelectedValue,
          EventCategory = _categoryTextBox.Text,
          UseEventCategoryColorAndMapFromCalendarColor = _mapColorCheckBox.Checked ,
          EventCategoryColor =  _categoryColorPicker.SelectedValue 
        };
      }
      set
      {
        _mapAttendeesCheckBox.Checked = value.MapAttendees;
        _mapBodyCheckBox.Checked = value.MapBody;
        _mapReminderComboBox.SelectedValue = value.MapReminder;
        _categoryTextBox.Text = value.EventCategory;
        _categoryColorPicker.SelectedValue = value.EventCategoryColor;
        _mapColorCheckBox.Checked = value.UseEventCategoryColorAndMapFromCalendarColor;

        UpdateCategoryColorControlsEnabled ();
      }
    }

    private async void _calendarColorRefreshButton_Click (object sender, EventArgs e)
    {
      string serverColor = await _calDavDataAccessFactory ().GetCalendarColorNoThrow ();

      if (!string.IsNullOrEmpty (serverColor))
      {
        Color c = ColorHelper.HexToColor (serverColor);
        _categoryColorPicker.SelectedValue = ColorHelper.FindMatchingCategoryColor (c);
      }
    }

    private void _mapColorCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      UpdateCategoryColorControlsEnabled ();
    }

    private void _categoryTextBox_TextChanged (object sender, EventArgs e)
    {
      UpdateCategoryColorControlsEnabled ();
    }

    private void UpdateCategoryColorControlsEnabled ()
    {
      if (!string.IsNullOrEmpty (_categoryTextBox.Text))
      {
        _mapColorCheckBox.Enabled = true;
        _calendarColorRefreshButton.Enabled = _mapColorCheckBox.Checked;
        _categoryColorPicker.Enabled = _mapColorCheckBox.Checked;
      }
      else
      {
        _mapColorCheckBox.Enabled = false;
        _calendarColorRefreshButton.Enabled = false;
        _categoryColorPicker.Enabled = false;
      }
    }
  }
}