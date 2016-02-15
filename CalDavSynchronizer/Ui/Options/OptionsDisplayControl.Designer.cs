using System;

namespace CalDavSynchronizer.Ui.Options
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
      this._mappingConfigurationButton = new System.Windows.Forms.Button();
      this._deleteButton = new System.Windows.Forms.Button();
      this._profileNameTextBox = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this._inactiveCheckBox = new System.Windows.Forms.CheckBox();
      this._copyButton = new System.Windows.Forms.Button();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this._browseToProfileCacheDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this._resetButton = new System.Windows.Forms.Button();
      this._outlookFolderControl = new CalDavSynchronizer.Ui.Options.OutlookFolderControl();
      this._serverSettingsControl = new CalDavSynchronizer.Ui.Options.ServerSettingsControl();
      this._syncSettingsControl = new CalDavSynchronizer.Ui.Options.SyncSettingsControl();
      this.contextMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // _mappingConfigurationButton
      // 
      this._mappingConfigurationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._mappingConfigurationButton.Location = new System.Drawing.Point(332, 638);
      this._mappingConfigurationButton.Margin = new System.Windows.Forms.Padding(4);
      this._mappingConfigurationButton.Name = "_mappingConfigurationButton";
      this._mappingConfigurationButton.Size = new System.Drawing.Size(172, 28);
      this._mappingConfigurationButton.TabIndex = 6;
      this._mappingConfigurationButton.Text = "Mapping configuration";
      this._mappingConfigurationButton.UseVisualStyleBackColor = true;
      this._mappingConfigurationButton.Click += new System.EventHandler(this._mappingConfigurationButton_Click);
      // 
      // _deleteButton
      // 
      this._deleteButton.Location = new System.Drawing.Point(16, 638);
      this._deleteButton.Margin = new System.Windows.Forms.Padding(4);
      this._deleteButton.Name = "_deleteButton";
      this._deleteButton.Size = new System.Drawing.Size(68, 28);
      this._deleteButton.TabIndex = 4;
      this._deleteButton.Text = "Delete";
      this._deleteButton.UseVisualStyleBackColor = true;
      this._deleteButton.Click += new System.EventHandler(this._deleteButton_Click);
      // 
      // _profileNameTextBox
      // 
      this._profileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._profileNameTextBox.Location = new System.Drawing.Point(326, 15);
      this._profileNameTextBox.Margin = new System.Windows.Forms.Padding(4);
      this._profileNameTextBox.Name = "_profileNameTextBox";
      this._profileNameTextBox.Size = new System.Drawing.Size(282, 22);
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
      this._inactiveCheckBox.Location = new System.Drawing.Point(512, 643);
      this._inactiveCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._inactiveCheckBox.Name = "_inactiveCheckBox";
      this._inactiveCheckBox.Size = new System.Drawing.Size(97, 21);
      this._inactiveCheckBox.TabIndex = 7;
      this._inactiveCheckBox.Text = "Deactivate";
      this._inactiveCheckBox.UseVisualStyleBackColor = true;
      // 
      // _copyButton
      // 
      this._copyButton.Location = new System.Drawing.Point(92, 638);
      this._copyButton.Margin = new System.Windows.Forms.Padding(4);
      this._copyButton.Name = "_copyButton";
      this._copyButton.Size = new System.Drawing.Size(59, 28);
      this._copyButton.TabIndex = 5;
      this._copyButton.Text = "Copy";
      this._copyButton.UseVisualStyleBackColor = true;
      this._copyButton.Click += new System.EventHandler(this._copyButton_Click);
      // 
      // contextMenu
      // 
      this.contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._browseToProfileCacheDirectoryToolStripMenuItem});
      this.contextMenu.Name = "contextMenu";
      this.contextMenu.Size = new System.Drawing.Size(298, 28);
      // 
      // _browseToProfileCacheDirectoryToolStripMenuItem
      // 
      this._browseToProfileCacheDirectoryToolStripMenuItem.Name = "_browseToProfileCacheDirectoryToolStripMenuItem";
      this._browseToProfileCacheDirectoryToolStripMenuItem.Size = new System.Drawing.Size(297, 24);
      this._browseToProfileCacheDirectoryToolStripMenuItem.Text = "Browse to profile cache directory";
      this._browseToProfileCacheDirectoryToolStripMenuItem.Click += new System.EventHandler(this._browseToProfileCacheDirectoryToolStripMenuItem_Click);
      // 
      // _resetButton
      // 
      this._resetButton.Location = new System.Drawing.Point(159, 638);
      this._resetButton.Margin = new System.Windows.Forms.Padding(4);
      this._resetButton.Name = "_resetButton";
      this._resetButton.Size = new System.Drawing.Size(104, 28);
      this._resetButton.TabIndex = 14;
      this._resetButton.Text = "Clear Cache";
      this._resetButton.UseVisualStyleBackColor = true;
      this._resetButton.Click += new System.EventHandler(this._resetButton_Click);
      // 
      // _outlookFolderControl
      // 
      this._outlookFolderControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._outlookFolderControl.Location = new System.Drawing.Point(16, 47);
      this._outlookFolderControl.Margin = new System.Windows.Forms.Padding(4);
      this._outlookFolderControl.Name = "_outlookFolderControl";
      this._outlookFolderControl.Size = new System.Drawing.Size(593, 98);
      this._outlookFolderControl.TabIndex = 1;
      // 
      // _serverSettingsControl
      // 
      this._serverSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._serverSettingsControl.Location = new System.Drawing.Point(16, 162);
      this._serverSettingsControl.Margin = new System.Windows.Forms.Padding(4);
      this._serverSettingsControl.Name = "_serverSettingsControl";
      this._serverSettingsControl.Size = new System.Drawing.Size(593, 214);
      this._serverSettingsControl.TabIndex = 2;
      // 
      // _syncSettingsControl
      // 
      this._syncSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._syncSettingsControl.BackColor = System.Drawing.SystemColors.Window;
      this._syncSettingsControl.Location = new System.Drawing.Point(16, 384);
      this._syncSettingsControl.Margin = new System.Windows.Forms.Padding(5);
      this._syncSettingsControl.Name = "_syncSettingsControl";
      this._syncSettingsControl.Size = new System.Drawing.Size(593, 246);
      this._syncSettingsControl.TabIndex = 3;
      // 
      // OptionsDisplayControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ContextMenuStrip = this.contextMenu;
      this.Controls.Add(this._resetButton);
      this.Controls.Add(this._outlookFolderControl);
      this.Controls.Add(this._mappingConfigurationButton);
      this.Controls.Add(this._copyButton);
      this.Controls.Add(this._inactiveCheckBox);
      this.Controls.Add(this.label10);
      this.Controls.Add(this._profileNameTextBox);
      this.Controls.Add(this._deleteButton);
      this.Controls.Add(this._serverSettingsControl);
      this.Controls.Add(this._syncSettingsControl);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "OptionsDisplayControl";
      this.Size = new System.Drawing.Size(625, 703);
      this.contextMenu.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _deleteButton;
    private System.Windows.Forms.TextBox _profileNameTextBox;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.CheckBox _inactiveCheckBox;
    private System.Windows.Forms.Button _copyButton;
    private SyncSettingsControl _syncSettingsControl;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Button _mappingConfigurationButton;
    private System.Windows.Forms.ContextMenuStrip contextMenu;
    private System.Windows.Forms.ToolStripMenuItem _browseToProfileCacheDirectoryToolStripMenuItem;
    private ServerSettingsControl _serverSettingsControl;
    private OutlookFolderControl _outlookFolderControl;
    private System.Windows.Forms.Button _resetButton;
  }
}
