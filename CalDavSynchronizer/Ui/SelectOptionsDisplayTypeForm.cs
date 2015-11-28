using System;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui
{
  public partial class SelectOptionsDisplayTypeForm : Form
  {
    public SelectOptionsDisplayTypeForm ()
    {
      InitializeComponent();
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public static OptionsDisplayType? QueryOptionsDisplayType ()
    {
      var form = new SelectOptionsDisplayTypeForm();
      if (form.ShowDialog() == DialogResult.OK)
      {
        if (form._genericTypeRadioButton.Checked)
          return OptionsDisplayType.Generic;

        if (form._googleTypeRadionButton.Checked)
          return OptionsDisplayType.Google;
      }

      return null;
    }
  }
}