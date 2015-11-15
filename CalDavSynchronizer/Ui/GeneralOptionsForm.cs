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
  public partial class GeneralOptionsForm : Form, IConfigurationForm<GeneralOptions>
  {
    public GeneralOptionsForm ()
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

    public GeneralOptions Options
    {
      get
      {
        return new GeneralOptions
               {
                   ShouldCheckForNewerVersions = _checkForNewerVersionsCheckBox.Checked,
               };
      }
      set { _checkForNewerVersionsCheckBox.Checked = value.ShouldCheckForNewerVersions; }
    }
  }
}