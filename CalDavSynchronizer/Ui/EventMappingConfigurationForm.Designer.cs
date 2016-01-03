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
      this.label3 = new System.Windows.Forms.Label();
      this._categoryShortcutKeycomboBox = new System.Windows.Forms.ComboBox();
      this._calendarColorSetButton = new System.Windows.Forms.Button();
      this._calendarColorRefreshButton = new System.Windows.Forms.Button();
      this._categoryColorPicker = new CalDavSynchronizer.Ui.ColorPicker();
      this._mapColorCheckBox = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this._sendNoAppointmentsNotificationsCheckBox = new System.Windows.Forms.CheckBox();
      this._scheduleAgentClientCheckBox = new System.Windows.Forms.CheckBox();
      this._outlookGroupBox.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(468, 298);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(75, 23);
      this._cancelButton.TabIndex = 13;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(387, 298);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(75, 23);
      this._okButton.TabIndex = 14;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _mapAttendeesCheckBox
      // 
      this._mapAttendeesCheckBox.AutoSize = true;
      this._mapAttendeesCheckBox.Location = new System.Drawing.Point(9, 19);
      this._mapAttendeesCheckBox.Name = "_mapAttendeesCheckBox";
      this._mapAttendeesCheckBox.Size = new System.Drawing.Size(167, 17);
      this._mapAttendeesCheckBox.TabIndex = 3;
      this._mapAttendeesCheckBox.Text = "Map Organizer and Attendees";
      this._mapAttendeesCheckBox.UseVisualStyleBackColor = true;
      this._mapAttendeesCheckBox.CheckedChanged += new System.EventHandler(this._mapAttendeesCheckBox_CheckedChanged);
      // 
      // _mapBodyCheckBox
      // 
      this._mapBodyCheckBox.AutoSize = true;
      this._mapBodyCheckBox.Location = new System.Drawing.Point(23, 40);
      this._mapBodyCheckBox.Name = "_mapBodyCheckBox";
      this._mapBodyCheckBox.Size = new System.Drawing.Size(74, 17);
      this._mapBodyCheckBox.TabIndex = 2;
      this._mapBodyCheckBox.Text = "Map Body";
      this._mapBodyCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapReminderComboBox
      // 
      this._mapReminderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._mapReminderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._mapReminderComboBox.FormattingEnabled = true;
      this._mapReminderComboBox.Location = new System.Drawing.Point(329, 12);
      this._mapReminderComboBox.Name = "_mapReminderComboBox";
      this._mapReminderComboBox.Size = new System.Drawing.Size(213, 21);
      this._mapReminderComboBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(74, 13);
      this.label1.TabIndex = 14;
      this.label1.Text = "Map reminder:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 26);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(189, 13);
      this.label2.TabIndex = 15;
      this.label2.Text = "Sync only Appointments with category:";
      // 
      // _categoryTextBox
      // 
      this._categoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._categoryTextBox.Location = new System.Drawing.Point(224, 23);
      this._categoryTextBox.Name = "_categoryTextBox";
      this._categoryTextBox.Size = new System.Drawing.Size(301, 20);
      this._categoryTextBox.TabIndex = 6;
      this._categoryTextBox.TextChanged += new System.EventHandler(this._categoryTextBox_TextChanged);
      // 
      // _outlookGroupBox
      // 
      this._outlookGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._outlookGroupBox.Controls.Add(this.label3);
      this._outlookGroupBox.Controls.Add(this._categoryShortcutKeycomboBox);
      this._outlookGroupBox.Controls.Add(this._calendarColorSetButton);
      this._outlookGroupBox.Controls.Add(this._calendarColorRefreshButton);
      this._outlookGroupBox.Controls.Add(this._categoryColorPicker);
      this._outlookGroupBox.Controls.Add(this._mapColorCheckBox);
      this._outlookGroupBox.Controls.Add(this.label2);
      this._outlookGroupBox.Controls.Add(this._categoryTextBox);
      this._outlookGroupBox.Location = new System.Drawing.Point(15, 171);
      this._outlookGroupBox.Name = "_outlookGroupBox";
      this._outlookGroupBox.Size = new System.Drawing.Size(531, 121);
      this._outlookGroupBox.TabIndex = 6;
      this._outlookGroupBox.TabStop = false;
      this._outlookGroupBox.Text = "Outlook settings";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 84);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(116, 13);
      this.label3.TabIndex = 16;
      this.label3.Text = "Category Shortcut Key:";
      // 
      // _categoryShortcutKeycomboBox
      // 
      this._categoryShortcutKeycomboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._categoryShortcutKeycomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._categoryShortcutKeycomboBox.FormattingEnabled = true;
      this._categoryShortcutKeycomboBox.Location = new System.Drawing.Point(224, 84);
      this._categoryShortcutKeycomboBox.Name = "_categoryShortcutKeycomboBox";
      this._categoryShortcutKeycomboBox.Size = new System.Drawing.Size(112, 21);
      this._categoryShortcutKeycomboBox.TabIndex = 12;
      // 
      // _calendarColorSetButton
      // 
      this._calendarColorSetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._calendarColorSetButton.Location = new System.Drawing.Point(430, 52);
      this._calendarColorSetButton.Name = "_calendarColorSetButton";
      this._calendarColorSetButton.Size = new System.Drawing.Size(94, 23);
      this._calendarColorSetButton.TabIndex = 11;
      this._calendarColorSetButton.Text = "Set DAV Color";
      this._calendarColorSetButton.UseVisualStyleBackColor = false;
      this._calendarColorSetButton.Click += new System.EventHandler(this._calendarColorSetButton_Click);
      // 
      // _calendarColorRefreshButton
      // 
      this._calendarColorRefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._calendarColorRefreshButton.Location = new System.Drawing.Point(350, 52);
      this._calendarColorRefreshButton.Name = "_calendarColorRefreshButton";
      this._calendarColorRefreshButton.Size = new System.Drawing.Size(74, 23);
      this._calendarColorRefreshButton.TabIndex = 10;
      this._calendarColorRefreshButton.Text = "Fetch Color";
      this._calendarColorRefreshButton.UseVisualStyleBackColor = false;
      this._calendarColorRefreshButton.Click += new System.EventHandler(this._calendarColorRefreshButton_Click);
      // 
      // _categoryColorPicker
      // 
      this._categoryColorPicker.BackColor = System.Drawing.Color.White;
      this._categoryColorPicker.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this._categoryColorPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._categoryColorPicker.FormattingEnabled = true;
      this._categoryColorPicker.Location = new System.Drawing.Point(224, 53);
      this._categoryColorPicker.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this._categoryColorPicker.Name = "_categoryColorPicker";
      this._categoryColorPicker.SelectedItem = null;
      this._categoryColorPicker.SelectedValue = Microsoft.Office.Interop.Outlook.OlCategoryColor.olCategoryColorNone;
      this._categoryColorPicker.Size = new System.Drawing.Size(112, 21);
      this._categoryColorPicker.TabIndex = 9;
      // 
      // _mapColorCheckBox
      // 
      this._mapColorCheckBox.AutoSize = true;
      this._mapColorCheckBox.Location = new System.Drawing.Point(8, 54);
      this._mapColorCheckBox.Name = "_mapColorCheckBox";
      this._mapColorCheckBox.Size = new System.Drawing.Size(201, 17);
      this._mapColorCheckBox.TabIndex = 8;
      this._mapColorCheckBox.Text = "Map Calendar Color to category color";
      this._mapColorCheckBox.UseVisualStyleBackColor = true;
      this._mapColorCheckBox.CheckedChanged += new System.EventHandler(this._mapColorCheckBox_CheckedChanged);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this._sendNoAppointmentsNotificationsCheckBox);
      this.groupBox1.Controls.Add(this._scheduleAgentClientCheckBox);
      this.groupBox1.Controls.Add(this._mapAttendeesCheckBox);
      this.groupBox1.Location = new System.Drawing.Point(14, 63);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(531, 94);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Scheduling settings";
      // 
      // _sendNoAppointmentsNotificationsCheckBox
      // 
      this._sendNoAppointmentsNotificationsCheckBox.AutoSize = true;
      this._sendNoAppointmentsNotificationsCheckBox.Location = new System.Drawing.Point(9, 66);
      this._sendNoAppointmentsNotificationsCheckBox.Name = "_sendNoAppointmentsNotificationsCheckBox";
      this._sendNoAppointmentsNotificationsCheckBox.Size = new System.Drawing.Size(258, 17);
      this._sendNoAppointmentsNotificationsCheckBox.TabIndex = 5;
      this._sendNoAppointmentsNotificationsCheckBox.Text = "Don\'t send appointment notifications (from SOGo)";
      this._sendNoAppointmentsNotificationsCheckBox.UseVisualStyleBackColor = true;
      // 
      // _scheduleAgentClientCheckBox
      // 
      this._scheduleAgentClientCheckBox.AutoSize = true;
      this._scheduleAgentClientCheckBox.Location = new System.Drawing.Point(9, 42);
      this._scheduleAgentClientCheckBox.Name = "_scheduleAgentClientCheckBox";
      this._scheduleAgentClientCheckBox.Size = new System.Drawing.Size(187, 17);
      this._scheduleAgentClientCheckBox.TabIndex = 4;
      this._scheduleAgentClientCheckBox.Text = "Set SCHEDULE-AGENT=CLIENT";
      this._scheduleAgentClientCheckBox.UseVisualStyleBackColor = true;
      // 
      // EventMappingConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(555, 333);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this._outlookGroupBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this._mapReminderComboBox);
      this.Controls.Add(this._mapBodyCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "EventMappingConfigurationForm";
      this.ShowIcon = false;
      this.Text = "Appointment Mapping";
      this._outlookGroupBox.ResumeLayout(false);
      this._outlookGroupBox.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
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
    private System.Windows.Forms.Button _calendarColorRefreshButton;
    private System.Windows.Forms.CheckBox _mapColorCheckBox;
    private ColorPicker _categoryColorPicker;
    private System.Windows.Forms.Button _calendarColorSetButton;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox _categoryShortcutKeycomboBox;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox _sendNoAppointmentsNotificationsCheckBox;
    private System.Windows.Forms.CheckBox _scheduleAgentClientCheckBox;
  }
}