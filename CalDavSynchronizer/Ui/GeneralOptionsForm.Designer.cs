namespace CalDavSynchronizer.Ui
{
  partial class GeneralOptionsForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralOptionsForm));
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._checkForNewerVersionsCheckBox = new System.Windows.Forms.CheckBox();
      this._storeDataInRoamingFolderCheckBox = new System.Windows.Forms.CheckBox();
      this._toolTip = new System.Windows.Forms.ToolTip(this.components);
      this._enableTls12Checkbox = new System.Windows.Forms.CheckBox();
      this._disableCertificateValidationCheckbox = new System.Windows.Forms.CheckBox();
      this._enableSsl3Checkbox = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this._fixInvalidSettingsCheckBox = new System.Windows.Forms.CheckBox();
      this._displayAllProfilesAsGenericCheckBox = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this._reportPopupModeComboBox = new System.Windows.Forms.ComboBox();
      this._reportLogModeComboBox = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this._maxReportAgeInDays = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(295, 340);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(2);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(75, 23);
      this._cancelButton.TabIndex = 0;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(214, 340);
      this._okButton.Margin = new System.Windows.Forms.Padding(2);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(75, 23);
      this._okButton.TabIndex = 1;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _checkForNewerVersionsCheckBox
      // 
      this._checkForNewerVersionsCheckBox.AutoSize = true;
      this._checkForNewerVersionsCheckBox.Location = new System.Drawing.Point(12, 12);
      this._checkForNewerVersionsCheckBox.Margin = new System.Windows.Forms.Padding(2);
      this._checkForNewerVersionsCheckBox.Name = "_checkForNewerVersionsCheckBox";
      this._checkForNewerVersionsCheckBox.Size = new System.Drawing.Size(210, 17);
      this._checkForNewerVersionsCheckBox.TabIndex = 3;
      this._checkForNewerVersionsCheckBox.Text = "Automatically check for newer versions";
      this._checkForNewerVersionsCheckBox.UseVisualStyleBackColor = true;
      // 
      // _storeDataInRoamingFolderCheckBox
      // 
      this._storeDataInRoamingFolderCheckBox.AutoSize = true;
      this._storeDataInRoamingFolderCheckBox.Location = new System.Drawing.Point(12, 35);
      this._storeDataInRoamingFolderCheckBox.Margin = new System.Windows.Forms.Padding(2);
      this._storeDataInRoamingFolderCheckBox.Name = "_storeDataInRoamingFolderCheckBox";
      this._storeDataInRoamingFolderCheckBox.Size = new System.Drawing.Size(155, 17);
      this._storeDataInRoamingFolderCheckBox.TabIndex = 4;
      this._storeDataInRoamingFolderCheckBox.Text = "Store data in roaming folder";
      this._toolTip.SetToolTip(this._storeDataInRoamingFolderCheckBox, "Changing this option requires a restart of Outlook.");
      this._storeDataInRoamingFolderCheckBox.UseVisualStyleBackColor = true;
      // 
      // _toolTip
      // 
      this._toolTip.AutoPopDelay = 30000;
      this._toolTip.InitialDelay = 500;
      this._toolTip.ReshowDelay = 100;
      // 
      // _enableTls12Checkbox
      // 
      this._enableTls12Checkbox.AutoSize = true;
      this._enableTls12Checkbox.Location = new System.Drawing.Point(9, 42);
      this._enableTls12Checkbox.Margin = new System.Windows.Forms.Padding(2);
      this._enableTls12Checkbox.Name = "_enableTls12Checkbox";
      this._enableTls12Checkbox.Size = new System.Drawing.Size(94, 17);
      this._enableTls12Checkbox.TabIndex = 9;
      this._enableTls12Checkbox.Text = "Enable TLS12";
      this._toolTip.SetToolTip(this._enableTls12Checkbox, "Disabling is a major security risk, not recommended!");
      this._enableTls12Checkbox.UseVisualStyleBackColor = true;
      // 
      // _disableCertificateValidationCheckbox
      // 
      this._disableCertificateValidationCheckbox.AutoSize = true;
      this._disableCertificateValidationCheckbox.Location = new System.Drawing.Point(9, 19);
      this._disableCertificateValidationCheckbox.Margin = new System.Windows.Forms.Padding(2);
      this._disableCertificateValidationCheckbox.Name = "_disableCertificateValidationCheckbox";
      this._disableCertificateValidationCheckbox.Size = new System.Drawing.Size(160, 17);
      this._disableCertificateValidationCheckbox.TabIndex = 8;
      this._disableCertificateValidationCheckbox.Text = "Disable Certificate Validation";
      this._toolTip.SetToolTip(this._disableCertificateValidationCheckbox, "Major security risk, not recommended!");
      this._disableCertificateValidationCheckbox.UseVisualStyleBackColor = true;
      // 
      // _enableSsl3Checkbox
      // 
      this._enableSsl3Checkbox.AutoSize = true;
      this._enableSsl3Checkbox.Location = new System.Drawing.Point(9, 66);
      this._enableSsl3Checkbox.Margin = new System.Windows.Forms.Padding(2);
      this._enableSsl3Checkbox.Name = "_enableSsl3Checkbox";
      this._enableSsl3Checkbox.Size = new System.Drawing.Size(88, 17);
      this._enableSsl3Checkbox.TabIndex = 10;
      this._enableSsl3Checkbox.Text = "Enable SSL3";
      this._toolTip.SetToolTip(this._enableSsl3Checkbox, "Major security risk, not recommended!");
      this._enableSsl3Checkbox.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this._disableCertificateValidationCheckbox);
      this.groupBox1.Controls.Add(this._enableSsl3Checkbox);
      this.groupBox1.Controls.Add(this._enableTls12Checkbox);
      this.groupBox1.Location = new System.Drawing.Point(3, 117);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
      this.groupBox1.Size = new System.Drawing.Size(375, 93);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "SSL/TLS settings";
      this._toolTip.SetToolTip(this.groupBox1, "Changing these options can be a major security risk, not recommended!");
      // 
      // _fixInvalidSettingsCheckBox
      // 
      this._fixInvalidSettingsCheckBox.AutoSize = true;
      this._fixInvalidSettingsCheckBox.Location = new System.Drawing.Point(12, 58);
      this._fixInvalidSettingsCheckBox.Margin = new System.Windows.Forms.Padding(2);
      this._fixInvalidSettingsCheckBox.Name = "_fixInvalidSettingsCheckBox";
      this._fixInvalidSettingsCheckBox.Size = new System.Drawing.Size(111, 17);
      this._fixInvalidSettingsCheckBox.TabIndex = 5;
      this._fixInvalidSettingsCheckBox.Text = "Fix invalid settings";
      this._toolTip.SetToolTip(this._fixInvalidSettingsCheckBox, resources.GetString("_fixInvalidSettingsCheckBox.ToolTip"));
      this._fixInvalidSettingsCheckBox.UseVisualStyleBackColor = true;
      // 
      // _displayAllProfilesAsGenericCheckBox
      // 
      this._displayAllProfilesAsGenericCheckBox.AutoSize = true;
      this._displayAllProfilesAsGenericCheckBox.Location = new System.Drawing.Point(12, 81);
      this._displayAllProfilesAsGenericCheckBox.Margin = new System.Windows.Forms.Padding(2);
      this._displayAllProfilesAsGenericCheckBox.Name = "_displayAllProfilesAsGenericCheckBox";
      this._displayAllProfilesAsGenericCheckBox.Size = new System.Drawing.Size(197, 17);
      this._displayAllProfilesAsGenericCheckBox.TabIndex = 6;
      this._displayAllProfilesAsGenericCheckBox.Text = "Display all profiles as generic profiles";
      this._displayAllProfilesAsGenericCheckBox.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this._maxReportAgeInDays);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this._reportPopupModeComboBox);
      this.groupBox2.Controls.Add(this._reportLogModeComboBox);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(3, 215);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(375, 110);
      this.groupBox2.TabIndex = 8;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Synchronization reports";
      // 
      // _reportPopupModeComboBox
      // 
      this._reportPopupModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._reportPopupModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._reportPopupModeComboBox.FormattingEnabled = true;
      this._reportPopupModeComboBox.Location = new System.Drawing.Point(144, 50);
      this._reportPopupModeComboBox.Name = "_reportPopupModeComboBox";
      this._reportPopupModeComboBox.Size = new System.Drawing.Size(218, 21);
      this._reportPopupModeComboBox.TabIndex = 3;
      // 
      // _reportLogModeComboBox
      // 
      this._reportLogModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._reportLogModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._reportLogModeComboBox.FormattingEnabled = true;
      this._reportLogModeComboBox.Location = new System.Drawing.Point(144, 23);
      this._reportLogModeComboBox.Name = "_reportLogModeComboBox";
      this._reportLogModeComboBox.Size = new System.Drawing.Size(218, 21);
      this._reportLogModeComboBox.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(9, 53);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(94, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Show immediately:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(28, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Log:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(9, 80);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(157, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Delete reports older than (days):";
      // 
      // _maxReportAgeInDays
      // 
      this._maxReportAgeInDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._maxReportAgeInDays.Location = new System.Drawing.Point(263, 77);
      this._maxReportAgeInDays.Name = "_maxReportAgeInDays";
      this._maxReportAgeInDays.Size = new System.Drawing.Size(99, 20);
      this._maxReportAgeInDays.TabIndex = 5;
      // 
      // GeneralOptionsForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(377, 367);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this._displayAllProfilesAsGenericCheckBox);
      this.Controls.Add(this._fixInvalidSettingsCheckBox);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this._storeDataInRoamingFolderCheckBox);
      this.Controls.Add(this._checkForNewerVersionsCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "GeneralOptionsForm";
      this.ShowIcon = false;
      this.Text = "General Options";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.CheckBox _checkForNewerVersionsCheckBox;
    private System.Windows.Forms.CheckBox _storeDataInRoamingFolderCheckBox;
    private System.Windows.Forms.ToolTip _toolTip;
    private System.Windows.Forms.CheckBox _enableTls12Checkbox;
    private System.Windows.Forms.CheckBox _disableCertificateValidationCheckbox;
    private System.Windows.Forms.CheckBox _enableSsl3Checkbox;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox _fixInvalidSettingsCheckBox;
    private System.Windows.Forms.CheckBox _displayAllProfilesAsGenericCheckBox;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ComboBox _reportPopupModeComboBox;
    private System.Windows.Forms.ComboBox _reportLogModeComboBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox _maxReportAgeInDays;
    private System.Windows.Forms.Label label3;
  }
}