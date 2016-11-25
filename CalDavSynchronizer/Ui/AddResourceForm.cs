using System;
using System.Drawing;
using System.Windows.Forms;
using CalDavSynchronizer.Ui.Options;

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

    private bool ValidateForm()
    {
      return !string.IsNullOrEmpty (_resourceNameTextBox.Text);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      if (ValidateForm())
      {
        ResourceName = _resourceNameTextBox.Text;
        UseRandomUri = _useRandomNameCheckBox.Checked;
        DialogResult = DialogResult.OK;
      }
      else
      {
        MessageBox.Show ("Resource Displayname must not be empty!", OptionTasks.CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
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
