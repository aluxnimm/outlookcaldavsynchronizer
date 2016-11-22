using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.Ui
{
  public partial class AddResourceForm : Form
  {
    public string ResourceName { get; private set; }
    public bool UseRandomUri { get; private set; }
    public Color CalendarColor { get; private set; }


    public AddResourceForm (bool enableColorSelect)
    {
      InitializeComponent();
      if (!enableColorSelect)
      {
        _resourceColorButton.Visible = false;
        label2.Visible = false;
      }
      else
      {
        CalendarColor = Color.LightBlue;
        _resourceColorButton.BackColor = CalendarColor;
      }
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

    private void _resourceColorButton_Click(object sender, EventArgs e)
    {
      using (var colorDialog = new ColorDialog())
      {
        colorDialog.Color = CalendarColor;
        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
          CalendarColor = colorDialog.Color;
          _resourceColorButton.BackColor = colorDialog.Color;
        }
      }
    }
  }
}
