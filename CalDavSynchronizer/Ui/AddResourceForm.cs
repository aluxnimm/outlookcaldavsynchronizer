using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalDavSynchronizer.Ui
{
  public partial class AddResourceForm : Form
  {
    public string ResourceName { get; private set; }
    public bool UseRandomUri { get; private set; }

    public AddResourceForm()
    {
      InitializeComponent();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      ResourceName = _resourceNameTextBox.Text;
      UseRandomUri = _useRandomNameCheckBox.Checked;
      DialogResult = DialogResult.OK;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }
  }
}
