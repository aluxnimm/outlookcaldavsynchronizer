namespace CalDavSynchronizer.Ui
{
  partial class GoogleServerSettingsControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent ()
    {
      this.components = new System.ComponentModel.Container();
      this.groupBox = new System.Windows.Forms.GroupBox();
      this.label11 = new System.Windows.Forms.Label();
      this._emailAddressTextBox = new System.Windows.Forms.TextBox();
      this._testConnectionButton = new System.Windows.Forms.Button();
      this._calenderUrlTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this._urlGroupBox = new System.Windows.Forms.GroupBox();
      this._specifyUrlManually = new System.Windows.Forms.CheckBox();
      this.groupBox.SuspendLayout();
      this._urlGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this._specifyUrlManually);
      this.groupBox.Controls.Add(this._urlGroupBox);
      this.groupBox.Controls.Add(this.label11);
      this.groupBox.Controls.Add(this._emailAddressTextBox);
      this.groupBox.Controls.Add(this._testConnectionButton);
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(445, 174);
      this.groupBox.TabIndex = 1;
      this.groupBox.TabStop = false;
      this.groupBox.Text = "Server settings";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(6, 98);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(75, 13);
      this.label11.TabIndex = 13;
      this.label11.Text = "Email address:";
      // 
      // _emailAddressTextBox
      // 
      this._emailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._emailAddressTextBox.Location = new System.Drawing.Point(252, 95);
      this._emailAddressTextBox.Name = "_emailAddressTextBox";
      this._emailAddressTextBox.Size = new System.Drawing.Size(187, 20);
      this._emailAddressTextBox.TabIndex = 3;
      // 
      // _testConnectionButton
      // 
      this._testConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._testConnectionButton.Location = new System.Drawing.Point(363, 145);
      this._testConnectionButton.Name = "_testConnectionButton";
      this._testConnectionButton.Size = new System.Drawing.Size(76, 23);
      this._testConnectionButton.TabIndex = 5;
      this._testConnectionButton.Text = "Test settings";
      this._testConnectionButton.UseVisualStyleBackColor = true;
      // 
      // _calenderUrlTextBox
      // 
      this._calenderUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._calenderUrlTextBox.Location = new System.Drawing.Point(60, 29);
      this._calenderUrlTextBox.Name = "_calenderUrlTextBox";
      this._calenderUrlTextBox.Size = new System.Drawing.Size(367, 20);
      this._calenderUrlTextBox.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 32);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(48, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "DAV Url:";
      // 
      // _urlGroupBox
      // 
      this._urlGroupBox.Controls.Add(this.label3);
      this._urlGroupBox.Controls.Add(this._calenderUrlTextBox);
      this._urlGroupBox.Location = new System.Drawing.Point(6, 20);
      this._urlGroupBox.Name = "_urlGroupBox";
      this._urlGroupBox.Size = new System.Drawing.Size(433, 62);
      this._urlGroupBox.TabIndex = 14;
      this._urlGroupBox.TabStop = false;
      // 
      // _specifyUrlManually
      // 
      this._specifyUrlManually.AutoSize = true;
      this._specifyUrlManually.Location = new System.Drawing.Point(13, 19);
      this._specifyUrlManually.Name = "_specifyUrlManually";
      this._specifyUrlManually.Size = new System.Drawing.Size(111, 17);
      this._specifyUrlManually.TabIndex = 15;
      this._specifyUrlManually.Text = "Enter Url manually";
      this._specifyUrlManually.UseVisualStyleBackColor = true;
      // 
      // GoogleServerSettingsControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox);
      this.Name = "GoogleServerSettingsControl";
      this.Size = new System.Drawing.Size(445, 174);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      this._urlGroupBox.ResumeLayout(false);
      this._urlGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox;
    private System.Windows.Forms.Button _testConnectionButton;
    private System.Windows.Forms.TextBox _calenderUrlTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox _emailAddressTextBox;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.CheckBox _specifyUrlManually;
    private System.Windows.Forms.GroupBox _urlGroupBox;
  }
}
