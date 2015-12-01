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

namespace CalDavSynchronizer.Ui
{
  public partial class EventMappingConfigurationForm : Form, IConfigurationForm<EventMappingConfiguration>
  {
    private readonly IList<Item<ReminderMapping>> _availableReminderMappings = new List<Item<ReminderMapping>>()
                                                                               {
                                                                                   new Item<ReminderMapping> (ReminderMapping.@true, "Yes"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.@false, "No"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders"),
                                                                               };

    public EventMappingConfigurationForm ()
    {
      InitializeComponent();
      Item.BindComboBox (_mapReminderComboBox, _availableReminderMappings);
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public bool Display ()
    {
      return ShowDialog() == DialogResult.OK;
    }

    public EventMappingConfiguration Options
    {
      get
      {
        return new EventMappingConfiguration
               {
                   MapAttendees = _mapAttendeesCheckBox.Checked,
                   MapBody = _mapBodyCheckBox.Checked,
                   MapReminder = (ReminderMapping) _mapReminderComboBox.SelectedValue
               };
      }
      set
      {
        _mapAttendeesCheckBox.Checked = value.MapAttendees;
        _mapBodyCheckBox.Checked = value.MapBody;
        _mapReminderComboBox.SelectedValue = value.MapReminder;
      }
    }
  }
}