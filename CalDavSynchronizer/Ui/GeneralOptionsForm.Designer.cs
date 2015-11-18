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
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._checkForNewerVersionsCheckBox = new System.Windows.Forms.CheckBox();
      this._storeDataInRoamingFolderCheckBox = new System.Windows.Forms.CheckBox();
      this._toolTip = new System.Windows.Forms.ToolTip(this.components);
      this._enableTls12Checkbox = new System.Windows.Forms.CheckBox();
      this._disableCertificateValidationCheckbox = new System.Windows.Forms.CheckBox();
      this._enableSsl3Checkbox = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(252, 286);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 0;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(144, 286);
      this._okButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 1;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _checkForNewerVersionsCheckBox
      // 
      this._checkForNewerVersionsCheckBox.AutoSize = true;
      this._checkForNewerVersionsCheckBox.Location = new System.Drawing.Point(16, 15);
      this._checkForNewerVersionsCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._checkForNewerVersionsCheckBox.Name = "_checkForNewerVersionsCheckBox";
      this._checkForNewerVersionsCheckBox.Size = new System.Drawing.Size(274, 21);
      this._checkForNewerVersionsCheckBox.TabIndex = 5;
      this._checkForNewerVersionsCheckBox.Text = "Automatically check for newer versions";
      this._checkForNewerVersionsCheckBox.UseVisualStyleBackColor = true;
      // 
      // _storeDataInRoamingFolderCheckBox
      // 
      this._storeDataInRoamingFolderCheckBox.AutoSize = true;
      this._storeDataInRoamingFolderCheckBox.Location = new System.Drawing.Point(16, 43);
      this._storeDataInRoamingFolderCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._storeDataInRoamingFolderCheckBox.Name = "_storeDataInRoamingFolderCheckBox";
      this._storeDataInRoamingFolderCheckBox.Size = new System.Drawing.Size(206, 21);
      this._storeDataInRoamingFolderCheckBox.TabIndex = 6;
      this._storeDataInRoamingFolderCheckBox.Text = "Store data in roaming folder";
      this._toolTip.SetToolTip(this._storeDataInRoamingFolderCheckBox, "Changing this option requires a restart of Outlook.");
      this._storeDataInRoamingFolderCheckBox.UseVisualStyleBackColor = true;
      // 
      // _enableTls12Checkbox
      // 
      this._enableTls12Checkbox.AutoSize = true;
      this._enableTls12Checkbox.Location = new System.Drawing.Point(12, 52);
      this._enableTls12Checkbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._enableTls12Checkbox.Name = "_enableTls12Checkbox";
      this._enableTls12Checkbox.Size = new System.Drawing.Size(120, 21);
      this._enableTls12Checkbox.TabIndex = 7;
      this._enableTls12Checkbox.Text = "Enable TLS12";
      this._toolTip.SetToolTip(this._enableTls12Checkbox, "Disabling is a major security risk, not recommended!");
      this._enableTls12Checkbox.UseVisualStyleBackColor = true;
      // 
      // _disableCertificateValidationCheckbox
      // 
      this._disableCertificateValidationCheckbox.AutoSize = true;
      this._disableCertificateValidationCheckbox.Location = new System.Drawing.Point(12, 23);
      this._disableCertificateValidationCheckbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._disableCertificateValidationCheckbox.Name = "_disableCertificateValidationCheckbox";
      this._disableCertificateValidationCheckbox.Size = new System.Drawing.Size(210, 21);
      this._disableCertificateValidationCheckbox.TabIndex = 8;
      this._disableCertificateValidationCheckbox.Text = "Disable Certificate Validation";
      this._toolTip.SetToolTip(this._disableCertificateValidationCheckbox, "Major security risk, not recommended!");
      this._disableCertificateValidationCheckbox.UseVisualStyleBackColor = true;
      // 
      // _enableSsl3Checkbox
      // 
      this._enableSsl3Checkbox.AutoSize = true;
      this._enableSsl3Checkbox.Location = new System.Drawing.Point(12, 81);
      this._enableSsl3Checkbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._enableSsl3Checkbox.Name = "_enableSsl3Checkbox";
      this._enableSsl3Checkbox.Size = new System.Drawing.Size(112, 21);
      this._enableSsl3Checkbox.TabIndex = 9;
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
      this.groupBox1.Location = new System.Drawing.Point(4, 94);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Size = new System.Drawing.Size(360, 114);
      this.groupBox1.TabIndex = 10;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "SSL/TLS settings";
      this._toolTip.SetToolTip(this.groupBox1, "Changing these options can be a major security risk, not recommended!");
      // 
      // GeneralOptionsForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(368, 327);
      this.ControlBox = false;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this._storeDataInRoamingFolderCheckBox);
      this.Controls.Add(this._checkForNewerVersionsCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "GeneralOptionsForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "General Options";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
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
  }
}