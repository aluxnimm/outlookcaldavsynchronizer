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
                                                                                   new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders")
                                                                               };
    private readonly IList<Item<OlCategoryShortcutKey>> _availableShortcutKeys = new List<Item<OlCategoryShortcutKey>>()
                                                                               {
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyNone, "None"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF2, "Ctrl+F2"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF3, "Ctrl+F3"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF4, "Ctrl+F4"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF5, "Ctrl+F5"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF6, "Ctrl+F6"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF7, "Ctrl+F7"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF8, "Ctrl+F8"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF9, "Ctrl+F9"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF10, "Ctrl+F10"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF11, "Ctrl+F11"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF12, "Ctrl+F12")
                                                                               };
    private readonly Func<ICalDavDataAccess> _calDavDataAccessFactory;

    public EventMappingConfigurationForm (Func<ICalDavDataAccess> calDavDataAccessFactory)
    {
      InitializeComponent ();
      Item.BindComboBox (_mapReminderComboBox, _availableReminderMappings);
      Item.BindComboBox (_categoryShortcutKeycomboBox, _availableShortcutKeys);

      _calDavDataAccessFactory = calDavDataAccessFactory;

      _categoryColorPicker.AddCategoryColors ();
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      StringBuilder errorMessageBuilder = new StringBuilder();
      if (OptionTasks.ValidateCategoryName (_categoryTextBox.Text, errorMessageBuilder))
      {
          DialogResult = DialogResult.OK;
      }
      else
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The category name is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
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
          MapSensitivityPrivateToClassConfidential = _mapSensitivityPrivateToClassConfindentialCheckBox.Checked,
          MapClassConfidentialToSensitivityPrivate = _mapClassConfidentialToSensitivityPrivateCheckBox.Checked,
          ScheduleAgentClient = _scheduleAgentClientCheckBox.Checked,
          SendNoAppointmentNotifications = _sendNoAppointmentsNotificationsCheckBox.Checked,
          MapBody = _mapBodyCheckBox.Checked,
          CreateEventsInUTC = _createInUTCCheckBox.Checked,
          MapReminder = (ReminderMapping) _mapReminderComboBox.SelectedValue,
          EventCategory = _categoryTextBox.Text,
          UseEventCategoryColorAndMapFromCalendarColor = _mapColorCheckBox.Checked ,
          EventCategoryColor =  _categoryColorPicker.SelectedValue,
          CategoryShortcutKey = (OlCategoryShortcutKey) _categoryShortcutKeycomboBox.SelectedValue
        };
      }
      set
      {
        _mapAttendeesCheckBox.Checked = value.MapAttendees;
        _mapSensitivityPrivateToClassConfindentialCheckBox.Checked = value.MapSensitivityPrivateToClassConfidential;
        _mapClassConfidentialToSensitivityPrivateCheckBox.Checked = value.MapClassConfidentialToSensitivityPrivate;
        _scheduleAgentClientCheckBox.Checked = value.ScheduleAgentClient;
        _sendNoAppointmentsNotificationsCheckBox.Checked = value.SendNoAppointmentNotifications;
        _mapBodyCheckBox.Checked = value.MapBody;
        _createInUTCCheckBox.Checked = value.CreateEventsInUTC;
        _mapReminderComboBox.SelectedValue = value.MapReminder;
        _categoryTextBox.Text = value.EventCategory;
        _categoryColorPicker.SelectedValue = value.EventCategoryColor;
        _mapColorCheckBox.Checked = value.UseEventCategoryColorAndMapFromCalendarColor;
        _categoryShortcutKeycomboBox.SelectedValue = value.CategoryShortcutKey;
        UpdateCategoryColorControlsEnabled ();
        UpdateSchedulingControlsEnabled();
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
        _calendarColorSetButton.Enabled = _mapColorCheckBox.Checked;
        _categoryColorPicker.Enabled = _mapColorCheckBox.Checked;
        _categoryShortcutKeycomboBox.Enabled = true;
      }
      else
      {
        _mapColorCheckBox.Enabled = false;
        _calendarColorRefreshButton.Enabled = false;
        _calendarColorSetButton.Enabled = false;
        _categoryColorPicker.Enabled = false;
        _categoryShortcutKeycomboBox.Enabled = false;
      }
    }

    private async void _calendarColorSetButton_Click(object sender, EventArgs e)
    {
      if (_categoryColorPicker.SelectedValue != OlCategoryColor.olCategoryColorNone)
      {
        if (await _calDavDataAccessFactory().SetCalendarColorNoThrow ((ColorHelper.CategoryColors[_categoryColorPicker.SelectedValue] + "ff")))
        {
          MessageBox.Show ("Successfully updated the server calendar color!");
        }
        else
        {
          MessageBox.Show ("Error updating the server calendar color!");
        }
      }
      else
      {
        MessageBox.Show ("No color set for updating the server calendar color!");
      }
    }

    private void _mapAttendeesCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      UpdateSchedulingControlsEnabled();
    }

    private void UpdateSchedulingControlsEnabled ()
    {
      _scheduleAgentClientCheckBox.Enabled = _mapAttendeesCheckBox.Checked;
      _sendNoAppointmentsNotificationsCheckBox.Enabled = _mapAttendeesCheckBox.Checked;
    }
  }
}