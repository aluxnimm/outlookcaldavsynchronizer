namespace CalDavSynchronizer.Ui
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
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this._mapBirthdayCheckBox = new System.Windows.Forms.CheckBox();
      this._mapContactPhotoCheckBox = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(252, 118);
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
      this._okButton.Location = new System.Drawing.Point(144, 118);
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
      this._mapContactPhotoCheckBox.UseVisualStyleBackColor = true;
      // 
      // ContactMappingConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(368, 161);
      this.Controls.Add(this._mapContactPhotoCheckBox);
      this.Controls.Add(this._mapBirthdayCheckBox);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "ContactMappingConfigurationForm";
      this.ShowIcon = false;
      this.Text = "Contact Mapping";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.CheckBox _mapBirthdayCheckBox;
    private System.Windows.Forms.CheckBox _mapContactPhotoCheckBox;
  }
}