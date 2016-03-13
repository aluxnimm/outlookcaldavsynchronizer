using System;

namespace CalDavSynchronizer.Ui.Options
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
      this._doAutodiscoveryButton = new System.Windows.Forms.Button();
      this._editUrlManuallyButton = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this._calenderUrlTextBox = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this._emailAddressTextBox = new System.Windows.Forms.TextBox();
      this._testConnectionButton = new System.Windows.Forms.Button();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this._networkAndProxyOptionsButton = new System.Windows.Forms.Button();
      this.groupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Controls.Add(this._networkAndProxyOptionsButton);
      this.groupBox.Controls.Add(this._doAutodiscoveryButton);
      this.groupBox.Controls.Add(this._editUrlManuallyButton);
      this.groupBox.Controls.Add(this.label3);
      this.groupBox.Controls.Add(this._calenderUrlTextBox);
      this.groupBox.Controls.Add(this.label11);
      this.groupBox.Controls.Add(this._emailAddressTextBox);
      this.groupBox.Controls.Add(this._testConnectionButton);
      this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox.Name = "groupBox";
      this.groupBox.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox.Size = new System.Drawing.Size(593, 172);
      this.groupBox.TabIndex = 1;
      this.groupBox.TabStop = false;
      this.groupBox.Text = "Server settings";
      // 
      // _doAutodiscoveryButton
      // 
      this._doAutodiscoveryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._doAutodiscoveryButton.Location = new System.Drawing.Point(310, 136);
      this._doAutodiscoveryButton.Margin = new System.Windows.Forms.Padding(4);
      this._doAutodiscoveryButton.Name = "_doAutodiscoveryButton";
      this._doAutodiscoveryButton.Size = new System.Drawing.Size(166, 28);
      this._doAutodiscoveryButton.TabIndex = 15;
      this._doAutodiscoveryButton.Text = "Do Autodiscovery";
      this._doAutodiscoveryButton.UseVisualStyleBackColor = true;
      this._doAutodiscoveryButton.Click += new System.EventHandler(this.DoAutodiscoveryButton_Click);
      // 
      // _editUrlManuallyButton
      // 
      this._editUrlManuallyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._editUrlManuallyButton.Location = new System.Drawing.Point(484, 60);
      this._editUrlManuallyButton.Margin = new System.Windows.Forms.Padding(4);
      this._editUrlManuallyButton.Name = "_editUrlManuallyButton";
      this._editUrlManuallyButton.Size = new System.Drawing.Size(100, 28);
      this._editUrlManuallyButton.TabIndex = 14;
      this._editUrlManuallyButton.Text = "Edit Url";
      this._editUrlManuallyButton.UseVisualStyleBackColor = true;
      this._editUrlManuallyButton.Click += new System.EventHandler(this._editUrlManuallyButton_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(8, 32);
      this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(91, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Detected Url:";
      // 
      // _calenderUrlTextBox
      // 
      this._calenderUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._calenderUrlTextBox.BackColor = System.Drawing.SystemColors.Window;
      this._calenderUrlTextBox.Location = new System.Drawing.Point(109, 28);
      this._calenderUrlTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._calenderUrlTextBox.Name = "_calenderUrlTextBox";
      this._calenderUrlTextBox.ReadOnly = true;
      this._calenderUrlTextBox.Size = new System.Drawing.Size(475, 22);
      this._calenderUrlTextBox.TabIndex = 0;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(8, 98);
      this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(101, 17);
      this.label11.TabIndex = 13;
      this.label11.Text = "Email address:";
      // 
      // _emailAddressTextBox
      // 
      this._emailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._emailAddressTextBox.Location = new System.Drawing.Point(310, 95);
      this._emailAddressTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._emailAddressTextBox.Name = "_emailAddressTextBox";
      this._emailAddressTextBox.Size = new System.Drawing.Size(274, 22);
      this._emailAddressTextBox.TabIndex = 3;
      // 
      // _testConnectionButton
      // 
      this._testConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._testConnectionButton.Location = new System.Drawing.Point(484, 136);
      this._testConnectionButton.Margin = new System.Windows.Forms.Padding(4);
      this._testConnectionButton.Name = "_testConnectionButton";
      this._testConnectionButton.Size = new System.Drawing.Size(101, 28);
      this._testConnectionButton.TabIndex = 5;
      this._testConnectionButton.Text = "Test settings";
      this._testConnectionButton.UseVisualStyleBackColor = true;
      // 
      // _networkAndProxyOptionsButton
      // 
      this._networkAndProxyOptionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._networkAndProxyOptionsButton.Location = new System.Drawing.Point(8, 136);
      this._networkAndProxyOptionsButton.Margin = new System.Windows.Forms.Padding(4);
      this._networkAndProxyOptionsButton.Name = "_networkAndProxyOptionsButton";
      this._networkAndProxyOptionsButton.Size = new System.Drawing.Size(217, 28);
      this._networkAndProxyOptionsButton.TabIndex = 16;
      this._networkAndProxyOptionsButton.Text = "Network and proxy options";
      this._networkAndProxyOptionsButton.UseVisualStyleBackColor = true;
      this._networkAndProxyOptionsButton.Click += new System.EventHandler(this._networkAndProxyOptionsButton_Click);
      // 
      // GoogleServerSettingsControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBox);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "GoogleServerSettingsControl";
      this.Size = new System.Drawing.Size(593, 172);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox;
    private System.Windows.Forms.Button _testConnectionButton;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox _emailAddressTextBox;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox _calenderUrlTextBox;
    private System.Windows.Forms.Button _editUrlManuallyButton;
    private System.Windows.Forms.Button _doAutodiscoveryButton;
    private System.Windows.Forms.Button _networkAndProxyOptionsButton;
  }
}
