using System;

namespace CalDavSynchronizer.Ui.Options
{
  partial class ServerSettingsControl
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
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this._passwordTextBox = new System.Windows.Forms.TextBox();
      this._userNameTextBox = new System.Windows.Forms.TextBox();
      this._calenderUrlTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.groupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.label11);
      this.groupBox.Controls.Add(this._emailAddressTextBox);
      this.groupBox.Controls.Add(this._testConnectionButton);
      this.groupBox.Controls.Add(this.label5);
      this.groupBox.Controls.Add(this.label4);
      this.groupBox.Controls.Add(this._passwordTextBox);
      this.groupBox.Controls.Add(this._userNameTextBox);
      this.groupBox.Controls.Add(this._calenderUrlTextBox);
      this.groupBox.Controls.Add(this.label3);
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
      this.label11.Location = new System.Drawing.Point(6, 104);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(75, 13);
      this.label11.TabIndex = 13;
      this.label11.Text = "Email address:";
      // 
      // _emailAddressTextBox
      // 
      this._emailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._emailAddressTextBox.Location = new System.Drawing.Point(256, 101);
      this._emailAddressTextBox.Name = "_emailAddressTextBox";
      this._emailAddressTextBox.Size = new System.Drawing.Size(183, 20);
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
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(6, 78);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(56, 13);
      this.label5.TabIndex = 11;
      this.label5.Text = "Password:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 52);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(58, 13);
      this.label4.TabIndex = 10;
      this.label4.Text = "Username:";
      // 
      // _passwordTextBox
      // 
      this._passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._passwordTextBox.Location = new System.Drawing.Point(256, 75);
      this._passwordTextBox.Name = "_passwordTextBox";
      this._passwordTextBox.PasswordChar = '*';
      this._passwordTextBox.Size = new System.Drawing.Size(183, 20);
      this._passwordTextBox.TabIndex = 2;
      // 
      // _userNameTextBox
      // 
      this._userNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._userNameTextBox.Location = new System.Drawing.Point(256, 49);
      this._userNameTextBox.Name = "_userNameTextBox";
      this._userNameTextBox.Size = new System.Drawing.Size(183, 20);
      this._userNameTextBox.TabIndex = 1;
      // 
      // _calenderUrlTextBox
      // 
      this._calenderUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._calenderUrlTextBox.Location = new System.Drawing.Point(58, 23);
      this._calenderUrlTextBox.Name = "_calenderUrlTextBox";
      this._calenderUrlTextBox.Size = new System.Drawing.Size(380, 20);
      this._calenderUrlTextBox.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 26);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(48, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "DAV Url:";
      // 
      // ServerSettingsControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox);
      this.Name = "ServerSettingsControl";
      this.Size = new System.Drawing.Size(445, 174);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox;
    private System.Windows.Forms.Button _testConnectionButton;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox _passwordTextBox;
    private System.Windows.Forms.TextBox _userNameTextBox;
    private System.Windows.Forms.TextBox _calenderUrlTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox _emailAddressTextBox;
    private System.Windows.Forms.ToolTip toolTip;
  }
}
