using CalDavSynchronizer.Globalization;

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
      this.btnOK.Location = new System.Drawing.Point(495, 529);
      this.btnOK.Margin = new System.Windows.Forms.Padding(4);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(100, 28);
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
      this._licenseTextBox.Location = new System.Drawing.Point(15, 198);
      this._licenseTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._licenseTextBox.Multiline = true;
      this._licenseTextBox.Name = "_licenseTextBox";
      this._licenseTextBox.ReadOnly = true;
      this._licenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this._licenseTextBox.Size = new System.Drawing.Size(579, 323);
      this._licenseTextBox.TabIndex = 1;
      this._licenseTextBox.Text = resources.GetString("_licenseTextBox.Text");
      this._licenseTextBox.WordWrap = false;
      // 
      // _captionLabel
      // 
      this._captionLabel.AutoSize = true;
      this._captionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this._captionLabel.Location = new System.Drawing.Point(9, 11);
      this._captionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._captionLabel.Name = "_captionLabel";
      this._captionLabel.Size = new System.Drawing.Size(289, 31);
      this._captionLabel.TabIndex = 2;
      this._captionLabel.Text = "CalDav Synchronizer";
      // 
      // _versionLabel
      // 
      this._versionLabel.AutoSize = true;
      this._versionLabel.Location = new System.Drawing.Point(12, 49);
      this._versionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._versionLabel.Name = "_versionLabel";
      this._versionLabel.Size = new System.Drawing.Size(82, 17);
      this._versionLabel.TabIndex = 3;
      // 
      // _linkLabelProject
      // 
      this._linkLabelProject.AutoSize = true;
      this._linkLabelProject.Location = new System.Drawing.Point(12, 75);
      this._linkLabelProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._linkLabelProject.Name = "_linkLabelProject";
      this._linkLabelProject.Size = new System.Drawing.Size(0, 17);
      this._linkLabelProject.TabIndex = 4;
      this._linkLabelProject.TabStop = true;
      this._linkLabelProject.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._linkLabelProject_LinkClicked);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 107);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 17);
      this.label1.TabIndex = 5;
      this.label1.Text = "Team:";
      // 
      // _linkLabelTeamMembers
      // 
      this._linkLabelTeamMembers.AutoSize = true;
      this._linkLabelTeamMembers.Location = new System.Drawing.Point(56, 107);
      this._linkLabelTeamMembers.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._linkLabelTeamMembers.Name = "_linkLabelTeamMembers";
      this._linkLabelTeamMembers.Size = new System.Drawing.Size(113, 17);
      this._linkLabelTeamMembers.TabIndex = 6;
      this._linkLabelTeamMembers.TabStop = true;
      this._linkLabelTeamMembers.Text = "<teamMembers>";
      // 
      // _logoPictureBox
      // 
      this._logoPictureBox.Location = new System.Drawing.Point(448, 11);
      this._logoPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this._logoPictureBox.Name = "_logoPictureBox";
      this._logoPictureBox.Size = new System.Drawing.Size(147, 102);
      this._logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._logoPictureBox.TabIndex = 7;
      this._logoPictureBox.TabStop = false;
      // 
      // _linkLabelPayPal
      // 
      this._linkLabelPayPal.AutoSize = true;
      this._linkLabelPayPal.Location = new System.Drawing.Point(13, 138);
      this._linkLabelPayPal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._linkLabelPayPal.Name = "_linkLabelPayPal";
      this._linkLabelPayPal.Size = new System.Drawing.Size(130, 17);
      this._linkLabelPayPal.TabIndex = 8;
      this._linkLabelPayPal.TabStop = true;
      this._linkLabelPayPal.Text = "Donate with PayPal";
      this._linkLabelPayPal.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPayPal_LinkClicked);
      // 
      // _linkLabelHelp
      // 
      this._linkLabelHelp.AutoSize = true;
      this._linkLabelHelp.Location = new System.Drawing.Point(13, 167);
      this._linkLabelHelp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._linkLabelHelp.Name = "_linkLabelHelp";
      this._linkLabelHelp.Size = new System.Drawing.Size(190, 17);
      this._linkLabelHelp.TabIndex = 9;
      this._linkLabelHelp.TabStop = true;
      this._linkLabelHelp.Text = "Documentation and Tutorials";
      this._linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
      // 
      // _checkForUpdatesButton
      // 
      this._checkForUpdatesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._checkForUpdatesButton.Location = new System.Drawing.Point(337, 529);
      this._checkForUpdatesButton.Margin = new System.Windows.Forms.Padding(4);
      this._checkForUpdatesButton.Name = "_checkForUpdatesButton";
      this._checkForUpdatesButton.Size = new System.Drawing.Size(149, 28);
      this._checkForUpdatesButton.TabIndex = 10;
      this._checkForUpdatesButton.Text = "Check for Updates";
      this._checkForUpdatesButton.UseVisualStyleBackColor = true;
      this._checkForUpdatesButton.Click += new System.EventHandler(this.CheckForUpdatesButton_Click);
      // 
      // AboutForm
      // 
      this.AcceptButton = this.btnOK;
      this.CancelButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ClientSize = new System.Drawing.Size(612, 562);
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
      this.Margin = new System.Windows.Forms.Padding(4);
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