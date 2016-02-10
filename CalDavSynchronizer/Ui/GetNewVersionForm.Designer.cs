namespace CalDavSynchronizer.Ui
{
  partial class GetNewVersionForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetNewVersionForm));
      this.btnOK = new System.Windows.Forms.Button();
      this._newFeaturesTextBox = new System.Windows.Forms.TextBox();
      this._captionLabel = new System.Windows.Forms.Label();
      this._currentVersionLabel = new System.Windows.Forms.Label();
      this._downloadNewVersionLinkLabel = new System.Windows.Forms.LinkLabel();
      this._doNotCheckForNewerVersionsLinkLabel = new System.Windows.Forms.LinkLabel();
      this._logoPictureBox = new System.Windows.Forms.PictureBox();
      this.WhatsNewLabel = new System.Windows.Forms.Label();
      this._ignoreThisVersionLinkLabel = new System.Windows.Forms.LinkLabel();
      this.installButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this._logoPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(749, 527);
      this.btnOK.Margin = new System.Windows.Forms.Padding(4);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(100, 28);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // _newFeaturesTextBox
      // 
      this._newFeaturesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._newFeaturesTextBox.Location = new System.Drawing.Point(16, 162);
      this._newFeaturesTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._newFeaturesTextBox.Multiline = true;
      this._newFeaturesTextBox.Name = "_newFeaturesTextBox";
      this._newFeaturesTextBox.ReadOnly = true;
      this._newFeaturesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this._newFeaturesTextBox.Size = new System.Drawing.Size(832, 356);
      this._newFeaturesTextBox.TabIndex = 1;
      this._newFeaturesTextBox.WordWrap = true;
      // 
      // _captionLabel
      // 
      this._captionLabel.AutoSize = true;
      this._captionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this._captionLabel.Location = new System.Drawing.Point(9, 11);
      this._captionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._captionLabel.Name = "_captionLabel";
      this._captionLabel.Size = new System.Drawing.Size(320, 31);
      this._captionLabel.TabIndex = 2;
      this._captionLabel.Text = "Version {0} is available!";
      // 
      // _currentVersionLabel
      // 
      this._currentVersionLabel.AutoSize = true;
      this._currentVersionLabel.Location = new System.Drawing.Point(12, 49);
      this._currentVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._currentVersionLabel.Name = "_currentVersionLabel";
      this._currentVersionLabel.Size = new System.Drawing.Size(131, 17);
      this._currentVersionLabel.TabIndex = 3;
      this._currentVersionLabel.Text = "Current version: {0}";
      // 
      // _downloadNewVersionLinkLabel
      // 
      this._downloadNewVersionLinkLabel.AutoSize = true;
      this._downloadNewVersionLinkLabel.Location = new System.Drawing.Point(12, 76);
      this._downloadNewVersionLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._downloadNewVersionLinkLabel.Name = "_downloadNewVersionLinkLabel";
      this._downloadNewVersionLinkLabel.Size = new System.Drawing.Size(149, 17);
      this._downloadNewVersionLinkLabel.TabIndex = 4;
      this._downloadNewVersionLinkLabel.TabStop = true;
      this._downloadNewVersionLinkLabel.Text = "Download new version";
      this._downloadNewVersionLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._downloadNewVersionLinkLabel_LinkClicked);
      // 
      // _doNotCheckForNewerVersionsLinkLabel
      // 
      this._doNotCheckForNewerVersionsLinkLabel.AutoSize = true;
      this._doNotCheckForNewerVersionsLinkLabel.Location = new System.Drawing.Point(12, 107);
      this._doNotCheckForNewerVersionsLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._doNotCheckForNewerVersionsLinkLabel.Name = "_doNotCheckForNewerVersionsLinkLabel";
      this._doNotCheckForNewerVersionsLinkLabel.Size = new System.Drawing.Size(263, 17);
      this._doNotCheckForNewerVersionsLinkLabel.TabIndex = 6;
      this._doNotCheckForNewerVersionsLinkLabel.TabStop = true;
      this._doNotCheckForNewerVersionsLinkLabel.Text = "Do not check for newer version anymore";
      this._doNotCheckForNewerVersionsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._doNotCheckForNewerVersionsLinkLabel_LinkClicked);
      // 
      // _logoPictureBox
      // 
      this._logoPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._logoPictureBox.Location = new System.Drawing.Point(706, 11);
      this._logoPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this._logoPictureBox.Name = "_logoPictureBox";
      this._logoPictureBox.Size = new System.Drawing.Size(147, 102);
      this._logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._logoPictureBox.TabIndex = 7;
      this._logoPictureBox.TabStop = false;
      // 
      // WhatsNewLabel
      // 
      this.WhatsNewLabel.AutoSize = true;
      this.WhatsNewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
      this.WhatsNewLabel.Location = new System.Drawing.Point(12, 138);
      this.WhatsNewLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.WhatsNewLabel.Name = "WhatsNewLabel";
      this.WhatsNewLabel.Size = new System.Drawing.Size(126, 20);
      this.WhatsNewLabel.TabIndex = 8;
      this.WhatsNewLabel.Text = "Whats\'s new?";
      // 
      // _ignoreThisVersionLinkLabel
      // 
      this._ignoreThisVersionLinkLabel.AutoSize = true;
      this._ignoreThisVersionLinkLabel.Location = new System.Drawing.Point(176, 76);
      this._ignoreThisVersionLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._ignoreThisVersionLinkLabel.Name = "_ignoreThisVersionLinkLabel";
      this._ignoreThisVersionLinkLabel.Size = new System.Drawing.Size(124, 17);
      this._ignoreThisVersionLinkLabel.TabIndex = 9;
      this._ignoreThisVersionLinkLabel.TabStop = true;
      this._ignoreThisVersionLinkLabel.Text = "Ignore this version";
      this._ignoreThisVersionLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._ignoreThisVersionLinkLabel_LinkClicked);
      // 
      // installButton
      // 
      this.installButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.installButton.Location = new System.Drawing.Point(598, 526);
      this.installButton.Margin = new System.Windows.Forms.Padding(4);
      this.installButton.Name = "installButton";
      this.installButton.Size = new System.Drawing.Size(143, 28);
      this.installButton.TabIndex = 10;
      this.installButton.Text = "Install new version";
      this.installButton.UseVisualStyleBackColor = true;
      this.installButton.Click += new System.EventHandler(this.installButton_Click);
      // 
      // GetNewVersionForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(865, 570);
      this.Controls.Add(this.installButton);
      this.Controls.Add(this._ignoreThisVersionLinkLabel);
      this.Controls.Add(this.WhatsNewLabel);
      this.Controls.Add(this._logoPictureBox);
      this.Controls.Add(this._doNotCheckForNewerVersionsLinkLabel);
      this.Controls.Add(this._downloadNewVersionLinkLabel);
      this.Controls.Add(this._currentVersionLabel);
      this.Controls.Add(this._captionLabel);
      this.Controls.Add(this._newFeaturesTextBox);
      this.Controls.Add(this.btnOK);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "GetNewVersionForm";
      this.Text = "CalDav Synchronizer";
      ((System.ComponentModel.ISupportInitialize)(this._logoPictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TextBox _newFeaturesTextBox;
    private System.Windows.Forms.Label _captionLabel;
    private System.Windows.Forms.Label _currentVersionLabel;
    private System.Windows.Forms.LinkLabel _downloadNewVersionLinkLabel;
    private System.Windows.Forms.LinkLabel _doNotCheckForNewerVersionsLinkLabel;
    private System.Windows.Forms.PictureBox _logoPictureBox;
    private System.Windows.Forms.Label WhatsNewLabel;
    private System.Windows.Forms.LinkLabel _ignoreThisVersionLinkLabel;
    private System.Windows.Forms.Button installButton;
  }
}