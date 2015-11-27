namespace CalDavSynchronizer.Ui
{
  partial class OptionsDisplayControl
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
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox = new System.Windows.Forms.CheckBox();
      this._selectOutlookFolderButton = new System.Windows.Forms.Button();
      this._outoookFolderNameTextBox = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this._useGoogleOAuthCheckBox = new System.Windows.Forms.CheckBox();
      this.label11 = new System.Windows.Forms.Label();
      this._emailAddressTextBox = new System.Windows.Forms.TextBox();
      this._testConnectionButton = new System.Windows.Forms.Button();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this._passwordTextBox = new System.Windows.Forms.TextBox();
      this._userNameTextBox = new System.Windows.Forms.TextBox();
      this._calenderUrlTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this._advancedOptionsButton = new System.Windows.Forms.Button();
      this._deleteButton = new System.Windows.Forms.Button();
      this._profileNameTextBox = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this._inactiveCheckBox = new System.Windows.Forms.CheckBox();
      this._copyButton = new System.Windows.Forms.Button();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this._browseToProfileCacheDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this._syncSettingsControl = new CalDavSynchronizer.Ui.SyncSettingsControl();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.contextMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox);
      this.groupBox2.Controls.Add(this._selectOutlookFolderButton);
      this.groupBox2.Controls.Add(this._outoookFolderNameTextBox);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Location = new System.Drawing.Point(12, 226);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(445, 80);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Outlook settings";
      // 
      // _synchronizeImmediatelyAfterOutlookItemChangeCheckBox
      // 
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.AutoSize = true;
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Location = new System.Drawing.Point(11, 51);
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Name = "_synchronizeImmediatelyAfterOutlookItemChangeCheckBox";
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Size = new System.Drawing.Size(231, 17);
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.TabIndex = 1;
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Text = "Synchronize items immediately after change";
      this.toolTip.SetToolTip(this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox, "Trigger a partial synchronization run immediately after an item is \r\ncreated, cha" +
        "nged or deleted in Outlook via the Inspector dialog, \r\nworks only for Appointmen" +
        "ts at the moment!");
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.UseVisualStyleBackColor = true;
      // 
      // _selectOutlookFolderButton
      // 
      this._selectOutlookFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._selectOutlookFolderButton.Location = new System.Drawing.Point(411, 21);
      this._selectOutlookFolderButton.Name = "_selectOutlookFolderButton";
      this._selectOutlookFolderButton.Size = new System.Drawing.Size(28, 23);
      this._selectOutlookFolderButton.TabIndex = 0;
      this._selectOutlookFolderButton.Text = "...";
      this._selectOutlookFolderButton.UseVisualStyleBackColor = true;
      // 
      // _outoookFolderNameTextBox
      // 
      this._outoookFolderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._outoookFolderNameTextBox.Location = new System.Drawing.Point(190, 23);
      this._outoookFolderNameTextBox.Name = "_outoookFolderNameTextBox";
      this._outoookFolderNameTextBox.ReadOnly = true;
      this._outoookFolderNameTextBox.Size = new System.Drawing.Size(215, 20);
      this._outoookFolderNameTextBox.TabIndex = 11;
      this._outoookFolderNameTextBox.TabStop = false;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(8, 26);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(76, 13);
      this.label9.TabIndex = 13;
      this.label9.Text = "Outlook Folder";
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this._useGoogleOAuthCheckBox);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.Controls.Add(this._emailAddressTextBox);
      this.groupBox1.Controls.Add(this._testConnectionButton);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this._passwordTextBox);
      this.groupBox1.Controls.Add(this._userNameTextBox);
      this.groupBox1.Controls.Add(this._calenderUrlTextBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Location = new System.Drawing.Point(12, 38);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(445, 174);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Server settings";
      // 
      // _useGoogleOAuthCheckBox
      // 
      this._useGoogleOAuthCheckBox.AutoSize = true;
      this._useGoogleOAuthCheckBox.Location = new System.Drawing.Point(8, 145);
      this._useGoogleOAuthCheckBox.Name = "_useGoogleOAuthCheckBox";
      this._useGoogleOAuthCheckBox.Size = new System.Drawing.Size(115, 17);
      this._useGoogleOAuthCheckBox.TabIndex = 4;
      this._useGoogleOAuthCheckBox.Text = "Use Google OAuth";
      this._useGoogleOAuthCheckBox.UseVisualStyleBackColor = true;
      this._useGoogleOAuthCheckBox.CheckedChanged += new System.EventHandler(this._useGoogleOAuthCheckBox_CheckedChanged);
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
      // _advancedOptionsButton
      // 
      this._advancedOptionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._advancedOptionsButton.Location = new System.Drawing.Point(228, 518);
      this._advancedOptionsButton.Name = "_advancedOptionsButton";
      this._advancedOptionsButton.Size = new System.Drawing.Size(108, 23);
      this._advancedOptionsButton.TabIndex = 6;
      this._advancedOptionsButton.Text = "Advanced options";
      this._advancedOptionsButton.UseVisualStyleBackColor = true;
      this._advancedOptionsButton.Click += new System.EventHandler(this._advancedSettingsButton_Click);
      // 
      // _deleteButton
      // 
      this._deleteButton.Location = new System.Drawing.Point(12, 518);
      this._deleteButton.Name = "_deleteButton";
      this._deleteButton.Size = new System.Drawing.Size(75, 23);
      this._deleteButton.TabIndex = 4;
      this._deleteButton.Text = "Delete";
      this._deleteButton.UseVisualStyleBackColor = true;
      this._deleteButton.Click += new System.EventHandler(this._deleteButton_Click);
      // 
      // _profileNameTextBox
      // 
      this._profileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._profileNameTextBox.Location = new System.Drawing.Point(235, 12);
      this._profileNameTextBox.Name = "_profileNameTextBox";
      this._profileNameTextBox.Size = new System.Drawing.Size(222, 20);
      this._profileNameTextBox.TabIndex = 0;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(20, 15);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(68, 13);
      this.label10.TabIndex = 13;
      this.label10.Text = "Profile name:";
      // 
      // _inactiveCheckBox
      // 
      this._inactiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._inactiveCheckBox.AutoSize = true;
      this._inactiveCheckBox.Location = new System.Drawing.Point(342, 522);
      this._inactiveCheckBox.Name = "_inactiveCheckBox";
      this._inactiveCheckBox.Size = new System.Drawing.Size(110, 17);
      this._inactiveCheckBox.TabIndex = 7;
      this._inactiveCheckBox.Text = "Deactivate Profile";
      this._inactiveCheckBox.UseVisualStyleBackColor = true;
      // 
      // _copyButton
      // 
      this._copyButton.Location = new System.Drawing.Point(93, 518);
      this._copyButton.Name = "_copyButton";
      this._copyButton.Size = new System.Drawing.Size(75, 23);
      this._copyButton.TabIndex = 5;
      this._copyButton.Text = "Copy";
      this._copyButton.UseVisualStyleBackColor = true;
      this._copyButton.Click += new System.EventHandler(this._copyButton_Click);
      // 
      // contextMenu
      // 
      this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._browseToProfileCacheDirectoryToolStripMenuItem});
      this.contextMenu.Name = "contextMenu";
      this.contextMenu.Size = new System.Drawing.Size(248, 26);
      // 
      // _browseToProfileCacheDirectoryToolStripMenuItem
      // 
      this._browseToProfileCacheDirectoryToolStripMenuItem.Name = "_browseToProfileCacheDirectoryToolStripMenuItem";
      this._browseToProfileCacheDirectoryToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
      this._browseToProfileCacheDirectoryToolStripMenuItem.Text = "Browse to profile cache directory";
      this._browseToProfileCacheDirectoryToolStripMenuItem.Click += new System.EventHandler(this._browseToProfileCacheDirectoryToolStripMenuItem_Click);
      // 
      // _syncSettingsControl
      // 
      this._syncSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._syncSettingsControl.Location = new System.Drawing.Point(12, 312);
      this._syncSettingsControl.Name = "_syncSettingsControl";
      this._syncSettingsControl.Size = new System.Drawing.Size(445, 200);
      this._syncSettingsControl.TabIndex = 3;
      // 
      // OptionsDisplayControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ContextMenuStrip = this.contextMenu;
      this.Controls.Add(this._advancedOptionsButton);
      this.Controls.Add(this._copyButton);
      this.Controls.Add(this._inactiveCheckBox);
      this.Controls.Add(this.label10);
      this.Controls.Add(this._profileNameTextBox);
      this.Controls.Add(this._deleteButton);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this._syncSettingsControl);
      this.Name = "OptionsDisplayControl";
      this.Size = new System.Drawing.Size(469, 571);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.contextMenu.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button _selectOutlookFolderButton;
    private System.Windows.Forms.TextBox _outoookFolderNameTextBox;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button _testConnectionButton;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox _passwordTextBox;
    private System.Windows.Forms.TextBox _userNameTextBox;
    private System.Windows.Forms.TextBox _calenderUrlTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button _deleteButton;
    private System.Windows.Forms.TextBox _profileNameTextBox;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox _emailAddressTextBox;
    private System.Windows.Forms.CheckBox _inactiveCheckBox;
    private System.Windows.Forms.Button _copyButton;
 private SyncSettingsControl _syncSettingsControl;
    private System.Windows.Forms.CheckBox _useGoogleOAuthCheckBox;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.CheckBox _synchronizeImmediatelyAfterOutlookItemChangeCheckBox;
    private System.Windows.Forms.Button _advancedOptionsButton;
    private System.Windows.Forms.ContextMenuStrip contextMenu;
    private System.Windows.Forms.ToolStripMenuItem _browseToProfileCacheDirectoryToolStripMenuItem;
  }
}
