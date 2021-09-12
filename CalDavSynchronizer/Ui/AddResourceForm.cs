using System;
using System.Drawing;
using System.Windows.Forms;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options;

namespace CalDavSynchronizer.Ui
{
    public partial class AddResourceForm : Form
    {
        public string ResourceName { get; private set; }
        public bool UseRandomUri { get; private set; }
        public Color CalendarColor { get; private set; }


        public AddResourceForm(bool enableColorSelect)
        {
            InitializeComponent();
            Text = Strings.Get($"Add Resource");
            label1.Text = Strings.Get($"Resource Displayname");
            btnOK.Text = Strings.Get($"OK");
            buttonCancel.Text = Strings.Get($"Cancel");
            _useRandomNameCheckBox.Text = Strings.Get($"Use random string for DAV resource Uri");
            _toolTip.SetToolTip(_useRandomNameCheckBox, Strings.Get($"If unchecked the displayname is used for creating the DAV resource Uri instead of a random string."));
            if (!enableColorSelect)
            {
                _resourceColorButton.Visible = false;
                label2.Visible = false;
            }
            else
            {
                CalendarColor = Color.LightBlue;
                _resourceColorButton.BackColor = CalendarColor;
                label2.Text = Strings.Get($"Calendar Color");
            }
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private bool ValidateForm()
        {
            return !string.IsNullOrEmpty(_resourceNameTextBox.Text);
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
                MessageBox.Show(Strings.Get($"Resource displayname must not be empty!"), OptionTasks.CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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