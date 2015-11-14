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
    public EventMappingConfigurationForm ()
    {
      InitializeComponent();
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
                   MapReminder = _mapReminderCheckBox.Checked,
               };
      }
      set
      {
        _mapAttendeesCheckBox.Checked = value.MapAttendees;
        _mapBodyCheckBox.Checked = value.MapBody;
        _mapReminderCheckBox.Checked = value.MapReminder;
      }
    }
  }
}