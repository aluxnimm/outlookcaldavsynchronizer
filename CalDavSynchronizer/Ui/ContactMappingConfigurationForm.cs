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
  public partial class ContactMappingConfigurationForm : Form, IConfigurationForm<ContactMappingConfiguration>
  {
    public ContactMappingConfigurationForm ()
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

    public ContactMappingConfiguration Options
    {
      get
      {
        return new ContactMappingConfiguration
               {
                   MapBirthday = _mapBirthdayCheckBox.Checked,
               };
      }
      set
      {
        _mapBirthdayCheckBox.Checked = value.MapBirthday;
      }
    }
  }
}