namespace CalDavSynchronizer.Ui
{
  partial class AboutForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
      this.btnOK = new System.Windows.Forms.Button();
      this._licenseTextBox = new System.Windows.Forms.TextBox();
      this._captionLabel = new System.Windows.Forms.Label();
      this._versionLabel = new System.Windows.Forms.Label();
      this._linkLabelProject = new System.Windows.Forms.LinkLabel();
      this.label1 = new System.Windows.Forms.Label();
      this._linkLabelTeamMembers = new System.Windows.Forms.LinkLabel();
      this._logoPictureBox = new System.Windows.Forms.PictureBox();
      this._linkLabelPayPal = new System.Windows.Forms.LinkLabel();
      this._linkLabelHelp = new System.Windows.Forms.LinkLabel();
      this._checkForUpdatesButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this._logoPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(371, 430);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // _licenseTextBox
      // 
      this._licenseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._licenseTextBox.BackColor = System.Drawing.SystemColors.Window;
      this._licenseTextBox.Location = new System.Drawing.Point(11, 161);
      this._licenseTextBox.Multiline = true;
      this._licenseTextBox.Name = "_licenseTextBox";
      this._licenseTextBox.ReadOnly = true;
      this._licenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this._licenseTextBox.Size = new System.Drawing.Size(435, 263);
      this._licenseTextBox.TabIndex = 1;
      this._licenseTextBox.Text = resources.GetString("_licenseTextBox.Text");
      this._licenseTextBox.WordWrap = false;
      // 
      // _captionLabel
      // 
      this._captionLabel.AutoSize = true;
      this._captionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this._captionLabel.Location = new System.Drawing.Point(7, 9);
      this._captionLabel.Name = "_captionLabel";
      this._captionLabel.Size = new System.Drawing.Size(236, 26);
      this._captionLabel.TabIndex = 2;
      this._captionLabel.Text = "CalDav Synchronizer";
      // 
      // _versionLabel
      // 
      this._versionLabel.AutoSize = true;
      this._versionLabel.Location = new System.Drawing.Point(9, 40);
      this._versionLabel.Name = "_versionLabel";
      this._versionLabel.Size = new System.Drawing.Size(62, 13);
      this._versionLabel.TabIndex = 3;
      this._versionLabel.Text = "Version: {0}";
      // 
      // _linkLabelProject
      // 
      this._linkLabelProject.AutoSize = true;
      this._linkLabelProject.Location = new System.Drawing.Point(9, 61);
      this._linkLabelProject.Name = "_linkLabelProject";
      this._linkLabelProject.Size = new System.Drawing.Size(290, 13);
      this._linkLabelProject.TabIndex = 4;
      this._linkLabelProject.TabStop = true;
      this._linkLabelProject.Text = "http://sourceforge.net/projects/outlookcaldavsynchronizer/";
      this._linkLabelProject.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkLabelProject_LinkClicked);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 87);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(37, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Team:";
      // 
      // _linkLabelTeamMembers
      // 
      this._linkLabelTeamMembers.AutoSize = true;
      this._linkLabelTeamMembers.Location = new System.Drawing.Point(42, 87);
      this._linkLabelTeamMembers.Name = "_linkLabelTeamMembers";
      this._linkLabelTeamMembers.Size = new System.Drawing.Size(85, 13);
      this._linkLabelTeamMembers.TabIndex = 6;
      this._linkLabelTeamMembers.TabStop = true;
      this._linkLabelTeamMembers.Text = "<teamMembers>";
      // 
      // _logoPictureBox
      // 
      this._logoPictureBox.Location = new System.Drawing.Point(336, 9);
      this._logoPictureBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this._logoPictureBox.Name = "_logoPictureBox";
      this._logoPictureBox.Size = new System.Drawing.Size(110, 83);
      this._logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._logoPictureBox.TabIndex = 7;
      this._logoPictureBox.TabStop = false;
      // 
      // _linkLabelPayPal
      // 
      this._linkLabelPayPal.AutoSize = true;
      this._linkLabelPayPal.Location = new System.Drawing.Point(10, 112);
      this._linkLabelPayPal.Name = "_linkLabelPayPal";
      this._linkLabelPayPal.Size = new System.Drawing.Size(100, 13);
      this._linkLabelPayPal.TabIndex = 8;
      this._linkLabelPayPal.TabStop = true;
      this._linkLabelPayPal.Text = "Donate with PayPal";
      this._linkLabelPayPal.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPayPal_LinkClicked);
      // 
      // _linkLabelHelp
      // 
      this._linkLabelHelp.AutoSize = true;
      this._linkLabelHelp.Location = new System.Drawing.Point(10, 136);
      this._linkLabelHelp.Name = "_linkLabelHelp";
      this._linkLabelHelp.Size = new System.Drawing.Size(102, 13);
      this._linkLabelHelp.TabIndex = 9;
      this._linkLabelHelp.TabStop = true;
      this._linkLabelHelp.Text = "Help Page and Wiki";
      this._linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
      // 
      // _checkForUpdatesButton
      // 
      this._checkForUpdatesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._checkForUpdatesButton.Location = new System.Drawing.Point(253, 430);
      this._checkForUpdatesButton.Name = "_checkForUpdatesButton";
      this._checkForUpdatesButton.Size = new System.Drawing.Size(112, 23);
      this._checkForUpdatesButton.TabIndex = 10;
      this._checkForUpdatesButton.Text = "Check for Updates";
      this._checkForUpdatesButton.UseVisualStyleBackColor = true;
      this._checkForUpdatesButton.Click += new System.EventHandler(this._checkForUpdatesButton_ClickAsync);
      // 
      // AboutForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ClientSize = new System.Drawing.Size(459, 457);
      this.Controls.Add(this._checkForUpdatesButton);
      this.Controls.Add(this._linkLabelHelp);
      this.Controls.Add(this._linkLabelPayPal);
      this.Controls.Add(this._logoPictureBox);
      this.Controls.Add(this._linkLabelTeamMembers);
      this.Controls.Add(this.label1);
      this.Controls.Add(this._linkLabelProject);
      this.Controls.Add(this._versionLabel);
      this.Controls.Add(this._captionLabel);
      this.Controls.Add(this._licenseTextBox);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutForm";
      this.Text = "About";
      ((System.ComponentModel.ISupportInitialize)(this._logoPictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TextBox _licenseTextBox;
    private System.Windows.Forms.Label _captionLabel;
    private System.Windows.Forms.Label _versionLabel;
    private System.Windows.Forms.LinkLabel _linkLabelProject;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.LinkLabel _linkLabelTeamMembers;
    private System.Windows.Forms.PictureBox _logoPictureBox;
    private System.Windows.Forms.LinkLabel _linkLabelPayPal;
    private System.Windows.Forms.LinkLabel _linkLabelHelp;
    private System.Windows.Forms.Button _checkForUpdatesButton;
  }
}