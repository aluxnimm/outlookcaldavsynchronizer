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
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(252, 169);
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
      this._okButton.Location = new System.Drawing.Point(144, 169);
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
      // GeneralOptionsForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(368, 210);
      this.ControlBox = false;
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
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.CheckBox _checkForNewerVersionsCheckBox;
    private System.Windows.Forms.CheckBox _storeDataInRoamingFolderCheckBox;
    private System.Windows.Forms.ToolTip _toolTip;
  }
}