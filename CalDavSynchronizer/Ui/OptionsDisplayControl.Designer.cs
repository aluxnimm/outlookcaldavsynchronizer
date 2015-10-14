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
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this._selectOutlookFolderButton = new System.Windows.Forms.Button();
      this._outoookFolderNameTextBox = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this._syncIntervalComboBox = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this._conflictResolutionComboBox = new System.Windows.Forms.ComboBox();
      this._synchronizationModeComboBox = new System.Windows.Forms.ComboBox();
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
      this.numberOfDaysInTheFuture = new System.Windows.Forms.TextBox();
      this.numberOfDaysInThePast = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this._deleteButton = new System.Windows.Forms.Button();
      this._profileNameTextBox = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this._inactiveCheckBox = new System.Windows.Forms.CheckBox();
      this._copyButton = new System.Windows.Forms.Button();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this._enableTimeRangeFilteringCheckBox = new System.Windows.Forms.CheckBox();
      this._timeRangeFilteringGroupBox = new System.Windows.Forms.GroupBox();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this._timeRangeFilteringGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this._selectOutlookFolderButton);
      this.groupBox2.Controls.Add(this._outoookFolderNameTextBox);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Location = new System.Drawing.Point(16, 278);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox2.Size = new System.Drawing.Size(593, 98);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Outlook settings";
      // 
      // _selectOutlookFolderButton
      // 
      this._selectOutlookFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._selectOutlookFolderButton.Location = new System.Drawing.Point(548, 26);
      this._selectOutlookFolderButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._selectOutlookFolderButton.Name = "_selectOutlookFolderButton";
      this._selectOutlookFolderButton.Size = new System.Drawing.Size(37, 28);
      this._selectOutlookFolderButton.TabIndex = 0;
      this._selectOutlookFolderButton.Text = "...";
      this._selectOutlookFolderButton.UseVisualStyleBackColor = true;
      // 
      // _outoookFolderNameTextBox
      // 
      this._outoookFolderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._outoookFolderNameTextBox.Location = new System.Drawing.Point(253, 28);
      this._outoookFolderNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._outoookFolderNameTextBox.Name = "_outoookFolderNameTextBox";
      this._outoookFolderNameTextBox.ReadOnly = true;
      this._outoookFolderNameTextBox.Size = new System.Drawing.Size(285, 22);
      this._outoookFolderNameTextBox.TabIndex = 13;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(11, 32);
      this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(101, 17);
      this.label9.TabIndex = 13;
      this.label9.Text = "Outlook Folder";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(8, 103);
      this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(226, 17);
      this.label8.TabIndex = 25;
      this.label8.Text = "Synchronization interval (minutes):";
      // 
      // _syncIntervalComboBox
      // 
      this._syncIntervalComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._syncIntervalComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._syncIntervalComboBox.FormattingEnabled = true;
      this._syncIntervalComboBox.Location = new System.Drawing.Point(377, 100);
      this._syncIntervalComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._syncIntervalComboBox.Name = "_syncIntervalComboBox";
      this._syncIntervalComboBox.Size = new System.Drawing.Size(207, 24);
      this._syncIntervalComboBox.TabIndex = 5;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(8, 70);
      this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(129, 17);
      this.label7.TabIndex = 23;
      this.label7.Text = "Conflict Resolution:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(8, 37);
      this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(152, 17);
      this.label6.TabIndex = 22;
      this.label6.Text = "Synchronization Mode:";
      // 
      // _conflictResolutionComboBox
      // 
      this._conflictResolutionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._conflictResolutionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._conflictResolutionComboBox.FormattingEnabled = true;
      this._conflictResolutionComboBox.Location = new System.Drawing.Point(253, 66);
      this._conflictResolutionComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._conflictResolutionComboBox.Name = "_conflictResolutionComboBox";
      this._conflictResolutionComboBox.Size = new System.Drawing.Size(331, 24);
      this._conflictResolutionComboBox.TabIndex = 4;
      // 
      // _synchronizationModeComboBox
      // 
      this._synchronizationModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._synchronizationModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._synchronizationModeComboBox.FormattingEnabled = true;
      this._synchronizationModeComboBox.Location = new System.Drawing.Point(253, 33);
      this._synchronizationModeComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._synchronizationModeComboBox.Name = "_synchronizationModeComboBox";
      this._synchronizationModeComboBox.Size = new System.Drawing.Size(331, 24);
      this._synchronizationModeComboBox.TabIndex = 3;
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
      this.groupBox1.Location = new System.Drawing.Point(16, 47);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Size = new System.Drawing.Size(593, 214);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Server settings";
      // 
      // _useGoogleOAuthCheckBox
      // 
      this._useGoogleOAuthCheckBox.AutoSize = true;
      this._useGoogleOAuthCheckBox.Location = new System.Drawing.Point(15, 183);
      this._useGoogleOAuthCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._useGoogleOAuthCheckBox.Name = "_useGoogleOAuthCheckBox";
      this._useGoogleOAuthCheckBox.Size = new System.Drawing.Size(149, 21);
      this._useGoogleOAuthCheckBox.TabIndex = 14;
      this._useGoogleOAuthCheckBox.Text = "Use Google OAuth";
      this._useGoogleOAuthCheckBox.UseVisualStyleBackColor = true;
      this._useGoogleOAuthCheckBox.CheckedChanged += new System.EventHandler(this._useGoogleOAuthCheckBox_CheckedChanged);
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(8, 128);
      this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(101, 17);
      this.label11.TabIndex = 13;
      this.label11.Text = "Email address:";
      // 
      // _emailAddressTextBox
      // 
      this._emailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._emailAddressTextBox.Location = new System.Drawing.Point(341, 124);
      this._emailAddressTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._emailAddressTextBox.Name = "_emailAddressTextBox";
      this._emailAddressTextBox.Size = new System.Drawing.Size(243, 22);
      this._emailAddressTextBox.TabIndex = 12;
      // 
      // _testConnectionButton
      // 
      this._testConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._testConnectionButton.Location = new System.Drawing.Point(452, 178);
      this._testConnectionButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._testConnectionButton.Name = "_testConnectionButton";
      this._testConnectionButton.Size = new System.Drawing.Size(133, 28);
      this._testConnectionButton.TabIndex = 3;
      this._testConnectionButton.Text = "Test settings";
      this._testConnectionButton.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(8, 96);
      this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(73, 17);
      this.label5.TabIndex = 11;
      this.label5.Text = "Password:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(8, 64);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(77, 17);
      this.label4.TabIndex = 10;
      this.label4.Text = "Username:";
      // 
      // _passwordTextBox
      // 
      this._passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._passwordTextBox.Location = new System.Drawing.Point(341, 92);
      this._passwordTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._passwordTextBox.Name = "_passwordTextBox";
      this._passwordTextBox.PasswordChar = '*';
      this._passwordTextBox.Size = new System.Drawing.Size(243, 22);
      this._passwordTextBox.TabIndex = 2;
      // 
      // _userNameTextBox
      // 
      this._userNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._userNameTextBox.Location = new System.Drawing.Point(341, 60);
      this._userNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._userNameTextBox.Name = "_userNameTextBox";
      this._userNameTextBox.Size = new System.Drawing.Size(243, 22);
      this._userNameTextBox.TabIndex = 1;
      // 
      // _calenderUrlTextBox
      // 
      this._calenderUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._calenderUrlTextBox.Location = new System.Drawing.Point(78, 28);
      this._calenderUrlTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._calenderUrlTextBox.Name = "_calenderUrlTextBox";
      this._calenderUrlTextBox.Size = new System.Drawing.Size(506, 22);
      this._calenderUrlTextBox.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(8, 32);
      this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(62, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "DAV Url:";
      // 
      // numberOfDaysInTheFuture
      // 
      this.numberOfDaysInTheFuture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numberOfDaysInTheFuture.Location = new System.Drawing.Point(412, 54);
      this.numberOfDaysInTheFuture.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.numberOfDaysInTheFuture.Name = "numberOfDaysInTheFuture";
      this.numberOfDaysInTheFuture.Size = new System.Drawing.Size(156, 22);
      this.numberOfDaysInTheFuture.TabIndex = 7;
      this.numberOfDaysInTheFuture.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // numberOfDaysInThePast
      // 
      this.numberOfDaysInThePast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numberOfDaysInThePast.Location = new System.Drawing.Point(412, 22);
      this.numberOfDaysInThePast.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.numberOfDaysInThePast.Name = "numberOfDaysInThePast";
      this.numberOfDaysInThePast.Size = new System.Drawing.Size(156, 22);
      this.numberOfDaysInThePast.TabIndex = 6;
      this.numberOfDaysInThePast.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(8, 58);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(240, 17);
      this.label2.TabIndex = 16;
      this.label2.Text = "Synchronize timespan future  (days):";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(8, 26);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(230, 17);
      this.label1.TabIndex = 15;
      this.label1.Text = "Synchronize timespan past  (days):";
      // 
      // _deleteButton
      // 
      this._deleteButton.Location = new System.Drawing.Point(16, 638);
      this._deleteButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._deleteButton.Name = "_deleteButton";
      this._deleteButton.Size = new System.Drawing.Size(100, 28);
      this._deleteButton.TabIndex = 8;
      this._deleteButton.Text = "Delete";
      this._deleteButton.UseVisualStyleBackColor = true;
      this._deleteButton.Click += new System.EventHandler(this._deleteButton_Click);
      // 
      // _profileNameTextBox
      // 
      this._profileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._profileNameTextBox.Location = new System.Drawing.Point(313, 15);
      this._profileNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._profileNameTextBox.Name = "_profileNameTextBox";
      this._profileNameTextBox.Size = new System.Drawing.Size(295, 22);
      this._profileNameTextBox.TabIndex = 0;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(27, 18);
      this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(91, 17);
      this.label10.TabIndex = 13;
      this.label10.Text = "Profile name:";
      // 
      // _inactiveCheckBox
      // 
      this._inactiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._inactiveCheckBox.AutoSize = true;
      this._inactiveCheckBox.Location = new System.Drawing.Point(461, 642);
      this._inactiveCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._inactiveCheckBox.Name = "_inactiveCheckBox";
      this._inactiveCheckBox.Size = new System.Drawing.Size(141, 21);
      this._inactiveCheckBox.TabIndex = 26;
      this._inactiveCheckBox.Text = "Deactivate Profile";
      this._inactiveCheckBox.UseVisualStyleBackColor = true;
      // 
      // _copyButton
      // 
      this._copyButton.Location = new System.Drawing.Point(124, 638);
      this._copyButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._copyButton.Name = "_copyButton";
      this._copyButton.Size = new System.Drawing.Size(100, 28);
      this._copyButton.TabIndex = 27;
      this._copyButton.Text = "Copy";
      this._copyButton.UseVisualStyleBackColor = true;
      this._copyButton.Click += new System.EventHandler(this._copyButton_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this._enableTimeRangeFilteringCheckBox);
      this.groupBox3.Controls.Add(this._timeRangeFilteringGroupBox);
      this.groupBox3.Controls.Add(this.label8);
      this.groupBox3.Controls.Add(this._syncIntervalComboBox);
      this.groupBox3.Controls.Add(this.label7);
      this.groupBox3.Controls.Add(this._conflictResolutionComboBox);
      this.groupBox3.Controls.Add(this._synchronizationModeComboBox);
      this.groupBox3.Controls.Add(this.label6);
      this.groupBox3.Location = new System.Drawing.Point(16, 384);
      this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox3.Size = new System.Drawing.Size(593, 246);
      this.groupBox3.TabIndex = 28;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Sync settings";
      // 
      // _enableTimeRangeFilteringCheckBox
      // 
      this._enableTimeRangeFilteringCheckBox.AutoSize = true;
      this._enableTimeRangeFilteringCheckBox.Location = new System.Drawing.Point(15, 137);
      this._enableTimeRangeFilteringCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._enableTimeRangeFilteringCheckBox.Name = "_enableTimeRangeFilteringCheckBox";
      this._enableTimeRangeFilteringCheckBox.Size = new System.Drawing.Size(157, 21);
      this._enableTimeRangeFilteringCheckBox.TabIndex = 27;
      this._enableTimeRangeFilteringCheckBox.Text = "Use time range filter";
      this._enableTimeRangeFilteringCheckBox.UseVisualStyleBackColor = true;
      this._enableTimeRangeFilteringCheckBox.CheckedChanged += new System.EventHandler(this._enableTimeRangeFilteringCheckBox_CheckedChanged);
      // 
      // _timeRangeFilteringGroupBox
      // 
      this._timeRangeFilteringGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._timeRangeFilteringGroupBox.Controls.Add(this.label1);
      this._timeRangeFilteringGroupBox.Controls.Add(this.label2);
      this._timeRangeFilteringGroupBox.Controls.Add(this.numberOfDaysInThePast);
      this._timeRangeFilteringGroupBox.Controls.Add(this.numberOfDaysInTheFuture);
      this._timeRangeFilteringGroupBox.Location = new System.Drawing.Point(8, 139);
      this._timeRangeFilteringGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._timeRangeFilteringGroupBox.Name = "_timeRangeFilteringGroupBox";
      this._timeRangeFilteringGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._timeRangeFilteringGroupBox.Size = new System.Drawing.Size(577, 98);
      this._timeRangeFilteringGroupBox.TabIndex = 26;
      this._timeRangeFilteringGroupBox.TabStop = false;
      // 
      // OptionsDisplayControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this._copyButton);
      this.Controls.Add(this._inactiveCheckBox);
      this.Controls.Add(this.label10);
      this.Controls.Add(this._profileNameTextBox);
      this.Controls.Add(this._deleteButton);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox3);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "OptionsDisplayControl";
      this.Size = new System.Drawing.Size(625, 703);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this._timeRangeFilteringGroupBox.ResumeLayout(false);
      this._timeRangeFilteringGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button _selectOutlookFolderButton;
    private System.Windows.Forms.TextBox _outoookFolderNameTextBox;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox _syncIntervalComboBox;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ComboBox _conflictResolutionComboBox;
    private System.Windows.Forms.ComboBox _synchronizationModeComboBox;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button _testConnectionButton;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox _passwordTextBox;
    private System.Windows.Forms.TextBox _userNameTextBox;
    private System.Windows.Forms.TextBox _calenderUrlTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox numberOfDaysInTheFuture;
    private System.Windows.Forms.TextBox numberOfDaysInThePast;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button _deleteButton;
    private System.Windows.Forms.TextBox _profileNameTextBox;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox _emailAddressTextBox;
    private System.Windows.Forms.CheckBox _inactiveCheckBox;
    private System.Windows.Forms.Button _copyButton;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.GroupBox _timeRangeFilteringGroupBox;
    private System.Windows.Forms.CheckBox _enableTimeRangeFilteringCheckBox;
    private System.Windows.Forms.CheckBox _useGoogleOAuthCheckBox;
  }
}
