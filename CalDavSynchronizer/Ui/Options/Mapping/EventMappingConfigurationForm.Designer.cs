using System;

namespace CalDavSynchronizer.Ui.Options.Mapping
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EventMappingConfigurationForm));
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._mapAttendeesCheckBox = new System.Windows.Forms.CheckBox();
      this._mapBodyCheckBox = new System.Windows.Forms.CheckBox();
      this._mapReminderComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this._outlookGroupBox = new System.Windows.Forms.GroupBox();
      this._categoryNameComboBox = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this._categoryShortcutKeycomboBox = new System.Windows.Forms.ComboBox();
      this._calendarColorSetButton = new System.Windows.Forms.Button();
      this._calendarColorRefreshButton = new System.Windows.Forms.Button();
      this._mapColorCheckBox = new System.Windows.Forms.CheckBox();
      this._schedulingGroupBox = new System.Windows.Forms.GroupBox();
      this._sendNoAppointmentsNotificationsCheckBox = new System.Windows.Forms.CheckBox();
      this._scheduleAgentClientCheckBox = new System.Windows.Forms.CheckBox();
      this._createInUTCCheckBox = new System.Windows.Forms.CheckBox();
      this._toolTip = new System.Windows.Forms.ToolTip(this.components);
      this._privacyGroupBox = new System.Windows.Forms.GroupBox();
      this._mapClassConfidentialToSensitivityPrivateCheckBox = new System.Windows.Forms.CheckBox();
      this._mapSensitivityPrivateToClassConfindentialCheckBox = new System.Windows.Forms.CheckBox();
      this._invertCategoryFilterCheckBox = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this._categoryColorPicker = new CalDavSynchronizer.Ui.ColorPicker();
      this._outlookGroupBox.SuspendLayout();
      this._schedulingGroupBox.SuspendLayout();
      this._privacyGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(624, 545);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 14;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(516, 545);
      this._okButton.Margin = new System.Windows.Forms.Padding(4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 15;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _mapAttendeesCheckBox
      // 
      this._mapAttendeesCheckBox.AutoSize = true;
      this._mapAttendeesCheckBox.Location = new System.Drawing.Point(12, 23);
      this._mapAttendeesCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapAttendeesCheckBox.Name = "_mapAttendeesCheckBox";
      this._mapAttendeesCheckBox.Size = new System.Drawing.Size(220, 21);
      this._mapAttendeesCheckBox.TabIndex = 4;
      this._mapAttendeesCheckBox.Text = "Map Organizer and Attendees";
      this._mapAttendeesCheckBox.UseVisualStyleBackColor = true;
      this._mapAttendeesCheckBox.CheckedChanged += new System.EventHandler(this._mapAttendeesCheckBox_CheckedChanged);
      // 
      // _mapBodyCheckBox
      // 
      this._mapBodyCheckBox.AutoSize = true;
      this._mapBodyCheckBox.Location = new System.Drawing.Point(31, 49);
      this._mapBodyCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapBodyCheckBox.Name = "_mapBodyCheckBox";
      this._mapBodyCheckBox.Size = new System.Drawing.Size(93, 21);
      this._mapBodyCheckBox.TabIndex = 2;
      this._mapBodyCheckBox.Text = "Map Body";
      this._mapBodyCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapReminderComboBox
      // 
      this._mapReminderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._mapReminderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._mapReminderComboBox.FormattingEnabled = true;
      this._mapReminderComboBox.Location = new System.Drawing.Point(439, 15);
      this._mapReminderComboBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapReminderComboBox.Name = "_mapReminderComboBox";
      this._mapReminderComboBox.Size = new System.Drawing.Size(283, 24);
      this._mapReminderComboBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(28, 18);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(99, 17);
      this.label1.TabIndex = 15;
      this.label1.Text = "Map reminder:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(9, 64);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(147, 17);
      this.label2.TabIndex = 16;
      this.label2.Text = "Outlook category filter";
      // 
      // _outlookGroupBox
      // 
      this._outlookGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._outlookGroupBox.Controls.Add(this.label4);
      this._outlookGroupBox.Controls.Add(this._invertCategoryFilterCheckBox);
      this._outlookGroupBox.Controls.Add(this._categoryNameComboBox);
      this._outlookGroupBox.Controls.Add(this.label3);
      this._outlookGroupBox.Controls.Add(this._categoryShortcutKeycomboBox);
      this._outlookGroupBox.Controls.Add(this._calendarColorSetButton);
      this._outlookGroupBox.Controls.Add(this._calendarColorRefreshButton);
      this._outlookGroupBox.Controls.Add(this._categoryColorPicker);
      this._outlookGroupBox.Controls.Add(this._mapColorCheckBox);
      this._outlookGroupBox.Controls.Add(this.label2);
      this._outlookGroupBox.Location = new System.Drawing.Point(19, 314);
      this._outlookGroupBox.Margin = new System.Windows.Forms.Padding(4);
      this._outlookGroupBox.Name = "_outlookGroupBox";
      this._outlookGroupBox.Padding = new System.Windows.Forms.Padding(4);
      this._outlookGroupBox.Size = new System.Drawing.Size(708, 223);
      this._outlookGroupBox.TabIndex = 7;
      this._outlookGroupBox.TabStop = false;
      this._outlookGroupBox.Text = "Outlook settings";
      // 
      // _categoryNameComboBox
      // 
      this._categoryNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._categoryNameComboBox.FormattingEnabled = true;
      this._categoryNameComboBox.Location = new System.Drawing.Point(299, 61);
      this._categoryNameComboBox.Margin = new System.Windows.Forms.Padding(4);
      this._categoryNameComboBox.Name = "_categoryNameComboBox";
      this._categoryNameComboBox.Size = new System.Drawing.Size(399, 24);
      this._categoryNameComboBox.TabIndex = 7;
      this._toolTip.SetToolTip(this._categoryNameComboBox, "Enter the name of the Outlook category for filtering.\r\nIf the category doesn\'t ex" +
        "ist, it will be created in Outlook.");
      this._categoryNameComboBox.TextChanged += new System.EventHandler(this._categoryNameComboBox_TextChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(9, 182);
      this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(154, 17);
      this.label3.TabIndex = 17;
      this.label3.Text = "Category Shortcut Key:";
      // 
      // _categoryShortcutKeycomboBox
      // 
      this._categoryShortcutKeycomboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._categoryShortcutKeycomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._categoryShortcutKeycomboBox.FormattingEnabled = true;
      this._categoryShortcutKeycomboBox.Location = new System.Drawing.Point(299, 179);
      this._categoryShortcutKeycomboBox.Margin = new System.Windows.Forms.Padding(4);
      this._categoryShortcutKeycomboBox.Name = "_categoryShortcutKeycomboBox";
      this._categoryShortcutKeycomboBox.Size = new System.Drawing.Size(148, 24);
      this._categoryShortcutKeycomboBox.TabIndex = 13;
      // 
      // _calendarColorSetButton
      // 
      this._calendarColorSetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._calendarColorSetButton.Location = new System.Drawing.Point(573, 138);
      this._calendarColorSetButton.Margin = new System.Windows.Forms.Padding(4);
      this._calendarColorSetButton.Name = "_calendarColorSetButton";
      this._calendarColorSetButton.Size = new System.Drawing.Size(125, 28);
      this._calendarColorSetButton.TabIndex = 12;
      this._calendarColorSetButton.Text = "Set DAV Color";
      this._calendarColorSetButton.UseVisualStyleBackColor = false;
      this._calendarColorSetButton.Click += new System.EventHandler(this._calendarColorSetButton_Click);
      // 
      // _calendarColorRefreshButton
      // 
      this._calendarColorRefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._calendarColorRefreshButton.Location = new System.Drawing.Point(466, 138);
      this._calendarColorRefreshButton.Margin = new System.Windows.Forms.Padding(4);
      this._calendarColorRefreshButton.Name = "_calendarColorRefreshButton";
      this._calendarColorRefreshButton.Size = new System.Drawing.Size(99, 28);
      this._calendarColorRefreshButton.TabIndex = 11;
      this._calendarColorRefreshButton.Text = "Fetch Color";
      this._calendarColorRefreshButton.UseVisualStyleBackColor = false;
      this._calendarColorRefreshButton.Click += new System.EventHandler(this._calendarColorRefreshButton_Click);
      // 
      // _mapColorCheckBox
      // 
      this._mapColorCheckBox.AutoSize = true;
      this._mapColorCheckBox.Location = new System.Drawing.Point(12, 143);
      this._mapColorCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapColorCheckBox.Name = "_mapColorCheckBox";
      this._mapColorCheckBox.Size = new System.Drawing.Size(261, 21);
      this._mapColorCheckBox.TabIndex = 9;
      this._mapColorCheckBox.Text = "Map calendar color to category color";
      this._mapColorCheckBox.UseVisualStyleBackColor = true;
      this._mapColorCheckBox.CheckedChanged += new System.EventHandler(this._mapColorCheckBox_CheckedChanged);
      // 
      // _schedulingGroupBox
      // 
      this._schedulingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._schedulingGroupBox.Controls.Add(this._sendNoAppointmentsNotificationsCheckBox);
      this._schedulingGroupBox.Controls.Add(this._scheduleAgentClientCheckBox);
      this._schedulingGroupBox.Controls.Add(this._mapAttendeesCheckBox);
      this._schedulingGroupBox.Location = new System.Drawing.Point(19, 194);
      this._schedulingGroupBox.Margin = new System.Windows.Forms.Padding(4);
      this._schedulingGroupBox.Name = "_schedulingGroupBox";
      this._schedulingGroupBox.Padding = new System.Windows.Forms.Padding(4);
      this._schedulingGroupBox.Size = new System.Drawing.Size(708, 116);
      this._schedulingGroupBox.TabIndex = 6;
      this._schedulingGroupBox.TabStop = false;
      this._schedulingGroupBox.Text = "Scheduling settings";
      // 
      // _sendNoAppointmentsNotificationsCheckBox
      // 
      this._sendNoAppointmentsNotificationsCheckBox.AutoSize = true;
      this._sendNoAppointmentsNotificationsCheckBox.Location = new System.Drawing.Point(12, 81);
      this._sendNoAppointmentsNotificationsCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._sendNoAppointmentsNotificationsCheckBox.Name = "_sendNoAppointmentsNotificationsCheckBox";
      this._sendNoAppointmentsNotificationsCheckBox.Size = new System.Drawing.Size(344, 21);
      this._sendNoAppointmentsNotificationsCheckBox.TabIndex = 6;
      this._sendNoAppointmentsNotificationsCheckBox.Text = "Don\'t send appointment notifications (from SOGo)";
      this._sendNoAppointmentsNotificationsCheckBox.UseVisualStyleBackColor = true;
      // 
      // _scheduleAgentClientCheckBox
      // 
      this._scheduleAgentClientCheckBox.AutoSize = true;
      this._scheduleAgentClientCheckBox.Location = new System.Drawing.Point(12, 52);
      this._scheduleAgentClientCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._scheduleAgentClientCheckBox.Name = "_scheduleAgentClientCheckBox";
      this._scheduleAgentClientCheckBox.Size = new System.Drawing.Size(238, 21);
      this._scheduleAgentClientCheckBox.TabIndex = 5;
      this._scheduleAgentClientCheckBox.Text = "Set SCHEDULE-AGENT=CLIENT";
      this._scheduleAgentClientCheckBox.UseVisualStyleBackColor = true;
      // 
      // _createInUTCCheckBox
      // 
      this._createInUTCCheckBox.AutoSize = true;
      this._createInUTCCheckBox.Location = new System.Drawing.Point(31, 78);
      this._createInUTCCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._createInUTCCheckBox.Name = "_createInUTCCheckBox";
      this._createInUTCCheckBox.Size = new System.Drawing.Size(229, 21);
      this._createInUTCCheckBox.TabIndex = 3;
      this._createInUTCCheckBox.Text = "Create events on server in UTC";
      this._toolTip.SetToolTip(this._createInUTCCheckBox, resources.GetString("_createInUTCCheckBox.ToolTip"));
      this._createInUTCCheckBox.UseVisualStyleBackColor = true;
      // 
      // _toolTip
      // 
      this._toolTip.AutoPopDelay = 30000;
      this._toolTip.InitialDelay = 500;
      this._toolTip.ReshowDelay = 100;
      // 
      // _privacyGroupBox
      // 
      this._privacyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._privacyGroupBox.Controls.Add(this._mapClassConfidentialToSensitivityPrivateCheckBox);
      this._privacyGroupBox.Controls.Add(this._mapSensitivityPrivateToClassConfindentialCheckBox);
      this._privacyGroupBox.Location = new System.Drawing.Point(19, 107);
      this._privacyGroupBox.Margin = new System.Windows.Forms.Padding(4);
      this._privacyGroupBox.Name = "_privacyGroupBox";
      this._privacyGroupBox.Padding = new System.Windows.Forms.Padding(4);
      this._privacyGroupBox.Size = new System.Drawing.Size(708, 79);
      this._privacyGroupBox.TabIndex = 5;
      this._privacyGroupBox.TabStop = false;
      this._privacyGroupBox.Text = "Privacy settings";
      // 
      // _mapClassConfidentialToSensitivityPrivateCheckBox
      // 
      this._mapClassConfidentialToSensitivityPrivateCheckBox.AutoSize = true;
      this._mapClassConfidentialToSensitivityPrivateCheckBox.Location = new System.Drawing.Point(12, 52);
      this._mapClassConfidentialToSensitivityPrivateCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapClassConfidentialToSensitivityPrivateCheckBox.Name = "_mapClassConfidentialToSensitivityPrivateCheckBox";
      this._mapClassConfidentialToSensitivityPrivateCheckBox.Size = new System.Drawing.Size(528, 21);
      this._mapClassConfidentialToSensitivityPrivateCheckBox.TabIndex = 5;
      this._mapClassConfidentialToSensitivityPrivateCheckBox.Text = "Map DAV CLASS:CONFIDENTIAL to Outlook Private flag instead of Confidential";
      this._mapClassConfidentialToSensitivityPrivateCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapSensitivityPrivateToClassConfindentialCheckBox
      // 
      this._mapSensitivityPrivateToClassConfindentialCheckBox.AutoSize = true;
      this._mapSensitivityPrivateToClassConfindentialCheckBox.Location = new System.Drawing.Point(12, 23);
      this._mapSensitivityPrivateToClassConfindentialCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapSensitivityPrivateToClassConfindentialCheckBox.Name = "_mapSensitivityPrivateToClassConfindentialCheckBox";
      this._mapSensitivityPrivateToClassConfindentialCheckBox.Size = new System.Drawing.Size(512, 21);
      this._mapSensitivityPrivateToClassConfindentialCheckBox.TabIndex = 4;
      this._mapSensitivityPrivateToClassConfindentialCheckBox.Text = "Map Outlook Private flag to DAV CLASS:CONFIDENTIAL instead of PRIVATE";
      this._mapSensitivityPrivateToClassConfindentialCheckBox.UseVisualStyleBackColor = true;
      // 
      // _categoryNotFilterCheckBox
      // 
      this._invertCategoryFilterCheckBox.AutoSize = true;
      this._invertCategoryFilterCheckBox.Location = new System.Drawing.Point(12, 93);
      this._invertCategoryFilterCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._invertCategoryFilterCheckBox.Name = "_invertCategoryFilterCheckBox";
      this._invertCategoryFilterCheckBox.Size = new System.Drawing.Size(406, 21);
      this._invertCategoryFilterCheckBox.TabIndex = 19;
      this._invertCategoryFilterCheckBox.Text = "Negate filter and sync all Appointments except this category";
      this._invertCategoryFilterCheckBox.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(8, 30);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(584, 17);
      this.label4.TabIndex = 20;
      this.label4.Text = "Only sync Appointments with this category and assign this category to newly creat" +
    "ed events";
      // 
      // _categoryColorPicker
      // 
      this._categoryColorPicker.BackColor = System.Drawing.Color.White;
      this._categoryColorPicker.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this._categoryColorPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._categoryColorPicker.FormattingEnabled = true;
      this._categoryColorPicker.Location = new System.Drawing.Point(299, 141);
      this._categoryColorPicker.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this._categoryColorPicker.Name = "_categoryColorPicker";
      this._categoryColorPicker.SelectedItem = null;
      this._categoryColorPicker.SelectedValue = Microsoft.Office.Interop.Outlook.OlCategoryColor.olCategoryColorNone;
      this._categoryColorPicker.Size = new System.Drawing.Size(148, 23);
      this._categoryColorPicker.TabIndex = 10;
      // 
      // EventMappingConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(740, 588);
      this.Controls.Add(this._privacyGroupBox);
      this.Controls.Add(this._createInUTCCheckBox);
      this.Controls.Add(this._schedulingGroupBox);
      this.Controls.Add(this._outlookGroupBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this._mapReminderComboBox);
      this.Controls.Add(this._mapBodyCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "EventMappingConfigurationForm";
      this.Text = "Appointment Mapping";
      this._outlookGroupBox.ResumeLayout(false);
      this._outlookGroupBox.PerformLayout();
      this._schedulingGroupBox.ResumeLayout(false);
      this._schedulingGroupBox.PerformLayout();
      this._privacyGroupBox.ResumeLayout(false);
      this._privacyGroupBox.PerformLayout();
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
    private System.Windows.Forms.GroupBox _outlookGroupBox;
    private System.Windows.Forms.Button _calendarColorRefreshButton;
    private System.Windows.Forms.CheckBox _mapColorCheckBox;
    private ColorPicker _categoryColorPicker;
    private System.Windows.Forms.Button _calendarColorSetButton;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox _categoryShortcutKeycomboBox;
    private System.Windows.Forms.GroupBox _schedulingGroupBox;
    private System.Windows.Forms.CheckBox _sendNoAppointmentsNotificationsCheckBox;
    private System.Windows.Forms.CheckBox _scheduleAgentClientCheckBox;
    private System.Windows.Forms.CheckBox _createInUTCCheckBox;
    private System.Windows.Forms.ToolTip _toolTip;
    private System.Windows.Forms.GroupBox _privacyGroupBox;
    private System.Windows.Forms.CheckBox _mapClassConfidentialToSensitivityPrivateCheckBox;
    private System.Windows.Forms.CheckBox _mapSensitivityPrivateToClassConfindentialCheckBox;
    private System.Windows.Forms.ComboBox _categoryNameComboBox;
    private System.Windows.Forms.CheckBox _invertCategoryFilterCheckBox;
    private System.Windows.Forms.Label label4;
  }
}