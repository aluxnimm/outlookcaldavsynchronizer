using System;

namespace CalDavSynchronizer.Ui.Options.Mapping
{
  partial class TaskMappingConfigurationForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskMappingConfigurationForm));
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._mapPriorityCheckBox = new System.Windows.Forms.CheckBox();
      this._mapBodyCheckBox = new System.Windows.Forms.CheckBox();
      this._mapReminderLabel = new System.Windows.Forms.Label();
      this._mapReminderComboBox = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(443, 118);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 0;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(335, 118);
      this._okButton.Margin = new System.Windows.Forms.Padding(4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 1;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _mapPriorityCheckBox
      // 
      this._mapPriorityCheckBox.AutoSize = true;
      this._mapPriorityCheckBox.Location = new System.Drawing.Point(13, 43);
      this._mapPriorityCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapPriorityCheckBox.Name = "_mapPriorityCheckBox";
      this._mapPriorityCheckBox.Size = new System.Drawing.Size(105, 21);
      this._mapPriorityCheckBox.TabIndex = 4;
      this._mapPriorityCheckBox.Text = "Map Priority";
      this._mapPriorityCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapBodyCheckBox
      // 
      this._mapBodyCheckBox.AutoSize = true;
      this._mapBodyCheckBox.Location = new System.Drawing.Point(13, 72);
      this._mapBodyCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapBodyCheckBox.Name = "_mapBodyCheckBox";
      this._mapBodyCheckBox.Size = new System.Drawing.Size(93, 21);
      this._mapBodyCheckBox.TabIndex = 5;
      this._mapBodyCheckBox.Text = "Map Body";
      this._mapBodyCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapReminderLabel
      // 
      this._mapReminderLabel.AutoSize = true;
      this._mapReminderLabel.Location = new System.Drawing.Point(13, 16);
      this._mapReminderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this._mapReminderLabel.Name = "_mapReminderLabel";
      this._mapReminderLabel.Size = new System.Drawing.Size(99, 17);
      this._mapReminderLabel.TabIndex = 3;
      this._mapReminderLabel.Text = "Map reminder:";
      // 
      // _mapReminderComboBox
      // 
      this._mapReminderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._mapReminderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._mapReminderComboBox.FormattingEnabled = true;
      this._mapReminderComboBox.Location = new System.Drawing.Point(263, 13);
      this._mapReminderComboBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapReminderComboBox.Name = "_mapReminderComboBox";
      this._mapReminderComboBox.Size = new System.Drawing.Size(283, 24);
      this._mapReminderComboBox.TabIndex = 2;
      // 
      // TaskMappingConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(559, 161);
      this.Controls.Add(this._mapReminderLabel);
      this.Controls.Add(this._mapReminderComboBox);
      this.Controls.Add(this._mapBodyCheckBox);
      this.Controls.Add(this._mapPriorityCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "TaskMappingConfigurationForm";
      this.Text = "Task Mapping";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.CheckBox _mapPriorityCheckBox;
    private System.Windows.Forms.CheckBox _mapBodyCheckBox;
    private System.Windows.Forms.Label _mapReminderLabel;
    private System.Windows.Forms.ComboBox _mapReminderComboBox;
  }
}