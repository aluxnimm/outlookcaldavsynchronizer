namespace CalDavSynchronizer.Ui
{
  partial class EventMappingConfigurationForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._mapAttendeesCheckBox = new System.Windows.Forms.CheckBox();
      this._mapBodyCheckBox = new System.Windows.Forms.CheckBox();
      this._mapReminderComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this._categoryTextBox = new System.Windows.Forms.TextBox();
      this._outlookGroupBox = new System.Windows.Forms.GroupBox();
      this._calendarColorButton = new System.Windows.Forms.Button();
      this._mapColorCheckBox = new System.Windows.Forms.CheckBox();
      this._outlookGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(423, 244);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 0;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(315, 244);
      this._okButton.Margin = new System.Windows.Forms.Padding(4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 1;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _mapAttendeesCheckBox
      // 
      this._mapAttendeesCheckBox.AutoSize = true;
      this._mapAttendeesCheckBox.Location = new System.Drawing.Point(20, 52);
      this._mapAttendeesCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapAttendeesCheckBox.Name = "_mapAttendeesCheckBox";
      this._mapAttendeesCheckBox.Size = new System.Drawing.Size(125, 21);
      this._mapAttendeesCheckBox.TabIndex = 3;
      this._mapAttendeesCheckBox.Text = "Map Attendees";
      this._mapAttendeesCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapBodyCheckBox
      // 
      this._mapBodyCheckBox.AutoSize = true;
      this._mapBodyCheckBox.Location = new System.Drawing.Point(20, 80);
      this._mapBodyCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapBodyCheckBox.Name = "_mapBodyCheckBox";
      this._mapBodyCheckBox.Size = new System.Drawing.Size(93, 21);
      this._mapBodyCheckBox.TabIndex = 4;
      this._mapBodyCheckBox.Text = "Map Body";
      this._mapBodyCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapReminderComboBox
      // 
      this._mapReminderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._mapReminderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._mapReminderComboBox.FormattingEnabled = true;
      this._mapReminderComboBox.Location = new System.Drawing.Point(328, 15);
      this._mapReminderComboBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapReminderComboBox.Name = "_mapReminderComboBox";
      this._mapReminderComboBox.Size = new System.Drawing.Size(193, 24);
      this._mapReminderComboBox.TabIndex = 5;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 18);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(99, 17);
      this.label1.TabIndex = 6;
      this.label1.Text = "Map reminder:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(8, 32);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(250, 17);
      this.label2.TabIndex = 7;
      this.label2.Text = "Sync only Appointments with category:";
      // 
      // _categoryTextBox
      // 
      this._categoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._categoryTextBox.Location = new System.Drawing.Point(299, 28);
      this._categoryTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._categoryTextBox.Name = "_categoryTextBox";
      this._categoryTextBox.Size = new System.Drawing.Size(199, 22);
      this._categoryTextBox.TabIndex = 8;
      this._categoryTextBox.TextChanged += new System.EventHandler(this._categoryTextBox_TextChanged);
      // 
      // _outlookGroupBox
      // 
      this._outlookGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._outlookGroupBox.Controls.Add(this._calendarColorButton);
      this._outlookGroupBox.Controls.Add(this._mapColorCheckBox);
      this._outlookGroupBox.Controls.Add(this.label2);
      this._outlookGroupBox.Controls.Add(this._categoryTextBox);
      this._outlookGroupBox.Location = new System.Drawing.Point(16, 108);
      this._outlookGroupBox.Margin = new System.Windows.Forms.Padding(4);
      this._outlookGroupBox.Name = "_outlookGroupBox";
      this._outlookGroupBox.Padding = new System.Windows.Forms.Padding(4);
      this._outlookGroupBox.Size = new System.Drawing.Size(507, 115);
      this._outlookGroupBox.TabIndex = 9;
      this._outlookGroupBox.TabStop = false;
      this._outlookGroupBox.Text = "Outlook settings";
      // 
      // _calendarColorButton
      // 
      this._calendarColorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._calendarColorButton.Location = new System.Drawing.Point(299, 67);
      this._calendarColorButton.Margin = new System.Windows.Forms.Padding(4);
      this._calendarColorButton.Name = "_calendarColorButton";
      this._calendarColorButton.Size = new System.Drawing.Size(37, 28);
      this._calendarColorButton.TabIndex = 10;
      this._calendarColorButton.Text = "...";
      this._calendarColorButton.UseVisualStyleBackColor = false;
      this._calendarColorButton.Click += new System.EventHandler(this._calendarColorButton_Click);
      // 
      // _mapColorCheckBox
      // 
      this._mapColorCheckBox.AutoSize = true;
      this._mapColorCheckBox.Location = new System.Drawing.Point(11, 67);
      this._mapColorCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapColorCheckBox.Name = "_mapColorCheckBox";
      this._mapColorCheckBox.Size = new System.Drawing.Size(230, 21);
      this._mapColorCheckBox.TabIndex = 9;
      this._mapColorCheckBox.Text = "Map Calendar Color to category";
      this._mapColorCheckBox.UseVisualStyleBackColor = true;
      this._mapColorCheckBox.CheckedChanged += new System.EventHandler(this._mapColorCheckBox_CheckedChanged);
      // 
      // EventMappingConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(539, 287);
      this.Controls.Add(this._outlookGroupBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this._mapReminderComboBox);
      this.Controls.Add(this._mapBodyCheckBox);
      this.Controls.Add(this._mapAttendeesCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "EventMappingConfigurationForm";
      this.ShowIcon = false;
      this.Text = "Appointment Mapping";
      this._outlookGroupBox.ResumeLayout(false);
      this._outlookGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.CheckBox _mapAttendeesCheckBox;
    private System.Windows.Forms.CheckBox _mapBodyCheckBox;
    private System.Windows.Forms.ComboBox _mapReminderComboBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox _categoryTextBox;
    private System.Windows.Forms.GroupBox _outlookGroupBox;
    private System.Windows.Forms.Button _calendarColorButton;
    private System.Windows.Forms.CheckBox _mapColorCheckBox;
  }
}