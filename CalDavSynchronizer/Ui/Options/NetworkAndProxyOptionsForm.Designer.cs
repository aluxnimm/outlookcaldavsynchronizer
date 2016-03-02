using System;

namespace CalDavSynchronizer.Ui.Options
{
  partial class NetworkAndProxyOptionsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetworkAndProxyOptionsForm));
      this.OkButton = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this._useManualProxyCheckBox = new System.Windows.Forms.CheckBox();
      this._useSystemProxyCheckBox = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this._passwordTextBox = new System.Windows.Forms.TextBox();
      this._userNameTextBox = new System.Windows.Forms.TextBox();
      this._proxyUrlTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this._preemptiveAuthenticationCheckBox = new System.Windows.Forms.CheckBox();
      this._closeConnectionAfterEachRequestCheckBox = new System.Windows.Forms.CheckBox();
      this._manualProxyGroupBox = new System.Windows.Forms.GroupBox();
      this._forceBasicAuthenticationCheckBox = new System.Windows.Forms.CheckBox();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this._manualProxyGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // OkButton
      // 
      this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.OkButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.OkButton.Location = new System.Drawing.Point(485, 446);
      this.OkButton.Margin = new System.Windows.Forms.Padding(4);
      this.OkButton.Name = "OkButton";
      this.OkButton.Size = new System.Drawing.Size(100, 28);
      this.OkButton.TabIndex = 6;
      this.OkButton.Text = "OK";
      this.OkButton.UseVisualStyleBackColor = true;
      this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(599, 446);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(100, 28);
      this.buttonCancel.TabIndex = 7;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this._manualProxyGroupBox);
      this.groupBox2.Controls.Add(this._useManualProxyCheckBox);
      this.groupBox2.Controls.Add(this._useSystemProxyCheckBox);
      this.groupBox2.Location = new System.Drawing.Point(13, 159);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox2.Size = new System.Drawing.Size(685, 251);
      this.groupBox2.TabIndex = 9;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Proxy settings";
      // 
      // _useManualProxyCheckBox
      // 
      this._useManualProxyCheckBox.AutoSize = true;
      this._useManualProxyCheckBox.Location = new System.Drawing.Point(17, 52);
      this._useManualProxyCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._useManualProxyCheckBox.Name = "_useManualProxyCheckBox";
      this._useManualProxyCheckBox.Size = new System.Drawing.Size(230, 21);
      this._useManualProxyCheckBox.TabIndex = 16;
      this._useManualProxyCheckBox.Text = "Use Manual Proxy configuration";
      this._useManualProxyCheckBox.UseVisualStyleBackColor = true;
      this._useManualProxyCheckBox.CheckedChanged += new System.EventHandler(this._useManualProxyCheckBox_CheckedChanged);
      // 
      // _useSystemProxyCheckBox
      // 
      this._useSystemProxyCheckBox.AutoSize = true;
      this._useSystemProxyCheckBox.Location = new System.Drawing.Point(17, 23);
      this._useSystemProxyCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._useSystemProxyCheckBox.Name = "_useSystemProxyCheckBox";
      this._useSystemProxyCheckBox.Size = new System.Drawing.Size(396, 21);
      this._useSystemProxyCheckBox.TabIndex = 15;
      this._useSystemProxyCheckBox.Text = "Use System Default Proxy (settings from IE and config file)";
      this._useSystemProxyCheckBox.UseVisualStyleBackColor = true;
      this._useSystemProxyCheckBox.CheckedChanged += new System.EventHandler(this._useSystemProxyCheckBox_CheckedChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(6, 95);
      this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(137, 17);
      this.label5.TabIndex = 22;
      this.label5.Text = "Password (optional):";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 63);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(141, 17);
      this.label4.TabIndex = 21;
      this.label4.Text = "Username (optional):";
      // 
      // _passwordTextBox
      // 
      this._passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._passwordTextBox.Location = new System.Drawing.Point(409, 95);
      this._passwordTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._passwordTextBox.Name = "_passwordTextBox";
      this._passwordTextBox.PasswordChar = '*';
      this._passwordTextBox.Size = new System.Drawing.Size(243, 22);
      this._passwordTextBox.TabIndex = 20;
      // 
      // _userNameTextBox
      // 
      this._userNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._userNameTextBox.Location = new System.Drawing.Point(409, 63);
      this._userNameTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._userNameTextBox.Name = "_userNameTextBox";
      this._userNameTextBox.Size = new System.Drawing.Size(243, 22);
      this._userNameTextBox.TabIndex = 19;
      // 
      // _proxyUrlTextBox
      // 
      this._proxyUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._proxyUrlTextBox.Location = new System.Drawing.Point(155, 31);
      this._proxyUrlTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._proxyUrlTextBox.Name = "_proxyUrlTextBox";
      this._proxyUrlTextBox.Size = new System.Drawing.Size(497, 22);
      this._proxyUrlTextBox.TabIndex = 17;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 31);
      this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(69, 17);
      this.label3.TabIndex = 18;
      this.label3.Text = "Proxy Url:";
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this._forceBasicAuthenticationCheckBox);
      this.groupBox1.Controls.Add(this._preemptiveAuthenticationCheckBox);
      this.groupBox1.Controls.Add(this._closeConnectionAfterEachRequestCheckBox);
      this.groupBox1.Location = new System.Drawing.Point(13, 26);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox1.Size = new System.Drawing.Size(685, 125);
      this.groupBox1.TabIndex = 8;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Advanced network settings";
      // 
      // _preemptiveAuthenticationCheckBox
      // 
      this._preemptiveAuthenticationCheckBox.AutoSize = true;
      this._preemptiveAuthenticationCheckBox.Location = new System.Drawing.Point(17, 52);
      this._preemptiveAuthenticationCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._preemptiveAuthenticationCheckBox.Name = "_preemptiveAuthenticationCheckBox";
      this._preemptiveAuthenticationCheckBox.Size = new System.Drawing.Size(224, 21);
      this._preemptiveAuthenticationCheckBox.TabIndex = 17;
      this._preemptiveAuthenticationCheckBox.Text = "Use Preemptive Authentication";
      this._preemptiveAuthenticationCheckBox.UseVisualStyleBackColor = true;
      // 
      // _closeConnectionAfterEachRequestCheckBox
      // 
      this._closeConnectionAfterEachRequestCheckBox.AutoSize = true;
      this._closeConnectionAfterEachRequestCheckBox.Location = new System.Drawing.Point(17, 23);
      this._closeConnectionAfterEachRequestCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._closeConnectionAfterEachRequestCheckBox.Name = "_closeConnectionAfterEachRequestCheckBox";
      this._closeConnectionAfterEachRequestCheckBox.Size = new System.Drawing.Size(258, 21);
      this._closeConnectionAfterEachRequestCheckBox.TabIndex = 16;
      this._closeConnectionAfterEachRequestCheckBox.Text = "Close connection after each request";
      this._closeConnectionAfterEachRequestCheckBox.UseVisualStyleBackColor = true;
      // 
      // _manualProxyGroupBox
      // 
      this._manualProxyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._manualProxyGroupBox.Controls.Add(this.label5);
      this._manualProxyGroupBox.Controls.Add(this._passwordTextBox);
      this._manualProxyGroupBox.Controls.Add(this.label4);
      this._manualProxyGroupBox.Controls.Add(this.label3);
      this._manualProxyGroupBox.Controls.Add(this._proxyUrlTextBox);
      this._manualProxyGroupBox.Controls.Add(this._userNameTextBox);
      this._manualProxyGroupBox.Location = new System.Drawing.Point(17, 81);
      this._manualProxyGroupBox.Margin = new System.Windows.Forms.Padding(4);
      this._manualProxyGroupBox.Name = "_manualProxyGroupBox";
      this._manualProxyGroupBox.Padding = new System.Windows.Forms.Padding(4);
      this._manualProxyGroupBox.Size = new System.Drawing.Size(660, 139);
      this._manualProxyGroupBox.TabIndex = 10;
      this._manualProxyGroupBox.TabStop = false;
      // 
      // _forceBasicAuthenticationCheckBox
      // 
      this._forceBasicAuthenticationCheckBox.AutoSize = true;
      this._forceBasicAuthenticationCheckBox.Location = new System.Drawing.Point(17, 81);
      this._forceBasicAuthenticationCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._forceBasicAuthenticationCheckBox.Name = "_forceBasicAuthenticationCheckBox";
      this._forceBasicAuthenticationCheckBox.Size = new System.Drawing.Size(198, 21);
      this._forceBasicAuthenticationCheckBox.TabIndex = 18;
      this._forceBasicAuthenticationCheckBox.Text = "Force Basic Authentication";
      this._forceBasicAuthenticationCheckBox.UseVisualStyleBackColor = true;
      // 
      // NetworkAndProxyOptionsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ClientSize = new System.Drawing.Size(712, 486);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.OkButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.Name = "NetworkAndProxyOptionsForm";
      this.Text = "Network and Proxy Options";
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this._manualProxyGroupBox.ResumeLayout(false);
      this._manualProxyGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button OkButton;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox _useManualProxyCheckBox;
    private System.Windows.Forms.CheckBox _useSystemProxyCheckBox;
    private System.Windows.Forms.TextBox _proxyUrlTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox _passwordTextBox;
    private System.Windows.Forms.TextBox _userNameTextBox;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox _closeConnectionAfterEachRequestCheckBox;
    private System.Windows.Forms.GroupBox _manualProxyGroupBox;
    private System.Windows.Forms.CheckBox _preemptiveAuthenticationCheckBox;
    private System.Windows.Forms.CheckBox _forceBasicAuthenticationCheckBox;
  }
}