using System;

namespace CalDavSynchronizer.Ui.Options
{
  partial class SelectOptionsDisplayTypeForm
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
      this._genericTypeRadioButton = new System.Windows.Forms.RadioButton();
      this._googleTypeRadionButton = new System.Windows.Forms.RadioButton();
      this._cancelButton = new System.Windows.Forms.Button();
      this._okButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // _genericTypeRadioButton
      // 
      this._genericTypeRadioButton.AutoSize = true;
      this._genericTypeRadioButton.Checked = true;
      this._genericTypeRadioButton.Location = new System.Drawing.Point(12, 12);
      this._genericTypeRadioButton.Name = "_genericTypeRadioButton";
      this._genericTypeRadioButton.Size = new System.Drawing.Size(151, 17);
      this._genericTypeRadioButton.TabIndex = 0;
      this._genericTypeRadioButton.TabStop = true;
      this._genericTypeRadioButton.Text = "Generic CalDAV/CardDAV";
      this._genericTypeRadioButton.UseVisualStyleBackColor = true;
      // 
      // _googleTypeRadionButton
      // 
      this._googleTypeRadionButton.AutoSize = true;
      this._googleTypeRadionButton.Location = new System.Drawing.Point(12, 35);
      this._googleTypeRadionButton.Name = "_googleTypeRadionButton";
      this._googleTypeRadionButton.Size = new System.Drawing.Size(59, 17);
      this._googleTypeRadionButton.TabIndex = 1;
      this._googleTypeRadionButton.Text = "Google";
      this._googleTypeRadionButton.UseVisualStyleBackColor = true;
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(133, 77);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(75, 23);
      this._cancelButton.TabIndex = 2;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(52, 77);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(75, 23);
      this._okButton.TabIndex = 3;
      this._okButton.Text = "Ok";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // SelectOptionsDisplayTypeForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(220, 112);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.Controls.Add(this._googleTypeRadionButton);
      this.Controls.Add(this._genericTypeRadioButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "SelectOptionsDisplayTypeForm";
      this.ShowIcon = false;
      this.Text = "Select Profile Type";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton _genericTypeRadioButton;
    private System.Windows.Forms.RadioButton _googleTypeRadionButton;
    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
  }
}