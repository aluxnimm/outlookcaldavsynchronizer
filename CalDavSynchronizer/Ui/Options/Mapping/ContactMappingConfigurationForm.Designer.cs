using System;

namespace CalDavSynchronizer.Ui.Options.Mapping
{
  partial class ContactMappingConfigurationForm
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContactMappingConfigurationForm));
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._mapBirthdayCheckBox = new System.Windows.Forms.CheckBox();
      this._mapContactPhotoCheckBox = new System.Windows.Forms.CheckBox();
      this._fixPhoneNumberFormatCheckBox = new System.Windows.Forms.CheckBox();
      this._toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(252, 147);
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
      this._okButton.Location = new System.Drawing.Point(144, 147);
      this._okButton.Margin = new System.Windows.Forms.Padding(4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 1;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _mapBirthdayCheckBox
      // 
      this._mapBirthdayCheckBox.AutoSize = true;
      this._mapBirthdayCheckBox.Location = new System.Drawing.Point(16, 15);
      this._mapBirthdayCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapBirthdayCheckBox.Name = "_mapBirthdayCheckBox";
      this._mapBirthdayCheckBox.Size = new System.Drawing.Size(113, 21);
      this._mapBirthdayCheckBox.TabIndex = 2;
      this._mapBirthdayCheckBox.Text = "Map Birthday";
      this._toolTip.SetToolTip(this._mapBirthdayCheckBox, "Outlook automatically creates birthday appointments in your calendar\r\nwhen the bi" +
        "rthday is added to a contact item.");
      this._mapBirthdayCheckBox.UseVisualStyleBackColor = true;
      // 
      // _mapContactPhotoCheckBox
      // 
      this._mapContactPhotoCheckBox.AutoSize = true;
      this._mapContactPhotoCheckBox.Location = new System.Drawing.Point(16, 44);
      this._mapContactPhotoCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._mapContactPhotoCheckBox.Name = "_mapContactPhotoCheckBox";
      this._mapContactPhotoCheckBox.Size = new System.Drawing.Size(150, 21);
      this._mapContactPhotoCheckBox.TabIndex = 3;
      this._mapContactPhotoCheckBox.Text = "Map Contact Photo";
      this._toolTip.SetToolTip(this._mapContactPhotoCheckBox, "Not supported in OL 2007.");
      this._mapContactPhotoCheckBox.UseVisualStyleBackColor = true;
      // 
      // _fixPhoneNumberFormatCheckBox
      // 
      this._fixPhoneNumberFormatCheckBox.AutoSize = true;
      this._fixPhoneNumberFormatCheckBox.Location = new System.Drawing.Point(16, 73);
      this._fixPhoneNumberFormatCheckBox.Margin = new System.Windows.Forms.Padding(4);
      this._fixPhoneNumberFormatCheckBox.Name = "_fixPhoneNumberFormatCheckBox";
      this._fixPhoneNumberFormatCheckBox.Size = new System.Drawing.Size(246, 21);
      this._fixPhoneNumberFormatCheckBox.TabIndex = 4;
      this._fixPhoneNumberFormatCheckBox.Text = "Fix imported phone number format";
      this._toolTip.SetToolTip(this._fixPhoneNumberFormatCheckBox, "Convert numbers like +1 23 45678 9 \r\nto +1 (23) 45678 - 9 \r\nso that Outlook can s" +
        "how country and area code.");
      this._fixPhoneNumberFormatCheckBox.UseVisualStyleBackColor = true;
      // 
      // _toolTip
      // 
      this._toolTip.AutoPopDelay = 30000;
      this._toolTip.InitialDelay = 500;
      this._toolTip.ReshowDelay = 100;
      // 
      // ContactMappingConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(368, 190);
      this.Controls.Add(this._fixPhoneNumberFormatCheckBox);
      this.Controls.Add(this._mapContactPhotoCheckBox);
      this.Controls.Add(this._mapBirthdayCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "ContactMappingConfigurationForm";
      this.Text = "Contact Mapping";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.CheckBox _mapBirthdayCheckBox;
    private System.Windows.Forms.CheckBox _mapContactPhotoCheckBox;
    private System.Windows.Forms.CheckBox _fixPhoneNumberFormatCheckBox;
    private System.Windows.Forms.ToolTip _toolTip;
  }
}