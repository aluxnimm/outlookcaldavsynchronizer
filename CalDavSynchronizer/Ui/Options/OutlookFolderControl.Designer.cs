using System;

namespace CalDavSynchronizer.Ui.Options
{
  partial class OutlookFolderControl
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
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox = new System.Windows.Forms.CheckBox();
      this._selectOutlookFolderButton = new System.Windows.Forms.Button();
      this._outoookFolderNameTextBox = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.groupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox);
      this.groupBox.Controls.Add(this._selectOutlookFolderButton);
      this.groupBox.Controls.Add(this._outoookFolderNameTextBox);
      this.groupBox.Controls.Add(this.label9);
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox.Name = "groupBox";
      this.groupBox.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox.Size = new System.Drawing.Size(593, 98);
      this.groupBox.TabIndex = 2;
      this.groupBox.TabStop = false;
      this.groupBox.Text = "Outlook settings";
      // 
      // _synchronizeImmediatelyAfterOutlookItemChangeCheckBox
      // 
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.AutoSize = true;
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Location = new System.Drawing.Point(15, 63);
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Name = "_synchronizeImmediatelyAfterOutlookItemChangeCheckBox";
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Size = new System.Drawing.Size(307, 21);
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.TabIndex = 1;
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Text = "Synchronize items immediately after change";
      this.toolTip.SetToolTip(this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox, "Trigger a partial synchronization run immediately after \r\nan item is created, cha" +
        "nged or deleted in Outlook\r\n(with a 10 seconds delay).");
      this._synchronizeImmediatelyAfterOutlookItemChangeCheckBox.UseVisualStyleBackColor = true;
      // 
      // _selectOutlookFolderButton
      // 
      this._selectOutlookFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._selectOutlookFolderButton.Location = new System.Drawing.Point(548, 26);
      this._selectOutlookFolderButton.Margin = new System.Windows.Forms.Padding(4);
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
      this._outoookFolderNameTextBox.BackColor = System.Drawing.SystemColors.Window;
      this._outoookFolderNameTextBox.Location = new System.Drawing.Point(253, 28);
      this._outoookFolderNameTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._outoookFolderNameTextBox.Name = "_outoookFolderNameTextBox";
      this._outoookFolderNameTextBox.ReadOnly = true;
      this._outoookFolderNameTextBox.Size = new System.Drawing.Size(285, 22);
      this._outoookFolderNameTextBox.TabIndex = 11;
      this._outoookFolderNameTextBox.TabStop = false;
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
      // OutlookFolderControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBox);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "OutlookFolderControl";
      this.Size = new System.Drawing.Size(593, 98);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox;
    private System.Windows.Forms.Button _selectOutlookFolderButton;
    private System.Windows.Forms.TextBox _outoookFolderNameTextBox;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.CheckBox _synchronizeImmediatelyAfterOutlookItemChangeCheckBox;
  }
}
