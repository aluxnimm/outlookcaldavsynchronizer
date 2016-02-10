using System;

namespace CalDavSynchronizer.Ui.Options
{
  partial class SyncSettingsControl
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
      this.label8 = new System.Windows.Forms.Label();
      this._syncIntervalComboBox = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this._conflictResolutionComboBox = new System.Windows.Forms.ComboBox();
      this._synchronizationModeComboBox = new System.Windows.Forms.ComboBox();
      this.numberOfDaysInTheFuture = new System.Windows.Forms.TextBox();
      this.numberOfDaysInThePast = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox = new System.Windows.Forms.GroupBox();
      this._enableTimeRangeFilteringCheckBox = new System.Windows.Forms.CheckBox();
      this._timeRangeFilteringGroupBox = new System.Windows.Forms.GroupBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.groupBox.SuspendLayout();
      this._timeRangeFilteringGroupBox.SuspendLayout();
      this.SuspendLayout();
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
      this._syncIntervalComboBox.Location = new System.Drawing.Point(317, 100);
      this._syncIntervalComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._syncIntervalComboBox.Name = "_syncIntervalComboBox";
      this._syncIntervalComboBox.Size = new System.Drawing.Size(207, 24);
      this._syncIntervalComboBox.TabIndex = 3;
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
      this._conflictResolutionComboBox.Size = new System.Drawing.Size(271, 24);
      this._conflictResolutionComboBox.TabIndex = 2;
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
      this._synchronizationModeComboBox.Size = new System.Drawing.Size(271, 24);
      this._synchronizationModeComboBox.TabIndex = 1;
      // 
      // numberOfDaysInTheFuture
      // 
      this.numberOfDaysInTheFuture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numberOfDaysInTheFuture.Location = new System.Drawing.Point(352, 54);
      this.numberOfDaysInTheFuture.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.numberOfDaysInTheFuture.Name = "numberOfDaysInTheFuture";
      this.numberOfDaysInTheFuture.Size = new System.Drawing.Size(156, 22);
      this.numberOfDaysInTheFuture.TabIndex = 2;
      this.numberOfDaysInTheFuture.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // numberOfDaysInThePast
      // 
      this.numberOfDaysInThePast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.numberOfDaysInThePast.Location = new System.Drawing.Point(352, 22);
      this.numberOfDaysInThePast.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.numberOfDaysInThePast.Name = "numberOfDaysInThePast";
      this.numberOfDaysInThePast.Size = new System.Drawing.Size(156, 22);
      this.numberOfDaysInThePast.TabIndex = 1;
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
      // groupBox
      // 
      this.groupBox.Controls.Add(this._enableTimeRangeFilteringCheckBox);
      this.groupBox.Controls.Add(this._timeRangeFilteringGroupBox);
      this.groupBox.Controls.Add(this.label8);
      this.groupBox.Controls.Add(this._syncIntervalComboBox);
      this.groupBox.Controls.Add(this.label7);
      this.groupBox.Controls.Add(this._conflictResolutionComboBox);
      this.groupBox.Controls.Add(this._synchronizationModeComboBox);
      this.groupBox.Controls.Add(this.label6);
      this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox.Name = "groupBox";
      this.groupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox.Size = new System.Drawing.Size(533, 246);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      this.groupBox.Text = "Sync settings";
      // 
      // _enableTimeRangeFilteringCheckBox
      // 
      this._enableTimeRangeFilteringCheckBox.AutoSize = true;
      this._enableTimeRangeFilteringCheckBox.Location = new System.Drawing.Point(15, 137);
      this._enableTimeRangeFilteringCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._enableTimeRangeFilteringCheckBox.Name = "_enableTimeRangeFilteringCheckBox";
      this._enableTimeRangeFilteringCheckBox.Size = new System.Drawing.Size(157, 21);
      this._enableTimeRangeFilteringCheckBox.TabIndex = 4;
      this._enableTimeRangeFilteringCheckBox.Text = "Use time range filter";
      this.toolTip.SetToolTip(this._enableTimeRangeFilteringCheckBox, "Changing the time range filter setting leads to deletion of the sync cache\r\nand a" +
        " complete resync of the calendar!");
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
      this._timeRangeFilteringGroupBox.Size = new System.Drawing.Size(517, 98);
      this._timeRangeFilteringGroupBox.TabIndex = 5;
      this._timeRangeFilteringGroupBox.TabStop = false;
      // 
      // SyncSettingsControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBox);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "SyncSettingsControl";
      this.Size = new System.Drawing.Size(533, 246);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      this._timeRangeFilteringGroupBox.ResumeLayout(false);
      this._timeRangeFilteringGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox _syncIntervalComboBox;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ComboBox _conflictResolutionComboBox;
    private System.Windows.Forms.ComboBox _synchronizationModeComboBox;
    private System.Windows.Forms.TextBox numberOfDaysInTheFuture;
    private System.Windows.Forms.TextBox numberOfDaysInThePast;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox;
    private System.Windows.Forms.GroupBox _timeRangeFilteringGroupBox;
    private System.Windows.Forms.CheckBox _enableTimeRangeFilteringCheckBox;
    private System.Windows.Forms.ToolTip toolTip;
  }
}
