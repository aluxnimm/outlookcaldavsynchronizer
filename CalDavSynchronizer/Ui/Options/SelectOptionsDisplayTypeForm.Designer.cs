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
      this._fruuxTypeRadioButton = new System.Windows.Forms.RadioButton();
      this._posteoTypeRadioButton = new System.Windows.Forms.RadioButton();
      this._logoFruuxPictureBox = new System.Windows.Forms.PictureBox();
      this._logoGooglePictureBox = new System.Windows.Forms.PictureBox();
      this._logoPosteoPictureBox = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this._logoFruuxPictureBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this._logoGooglePictureBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this._logoPosteoPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // _genericTypeRadioButton
      // 
      this._genericTypeRadioButton.AutoSize = true;
      this._genericTypeRadioButton.Checked = true;
      this._genericTypeRadioButton.Location = new System.Drawing.Point(13, 13);
      this._genericTypeRadioButton.Margin = new System.Windows.Forms.Padding(4);
      this._genericTypeRadioButton.Name = "_genericTypeRadioButton";
      this._genericTypeRadioButton.Size = new System.Drawing.Size(193, 21);
      this._genericTypeRadioButton.TabIndex = 0;
      this._genericTypeRadioButton.TabStop = true;
      this._genericTypeRadioButton.Text = "Generic CalDAV/CardDAV";
      this._genericTypeRadioButton.UseVisualStyleBackColor = true;
      // 
      // _googleTypeRadionButton
      // 
      this._googleTypeRadionButton.AutoSize = true;
      this._googleTypeRadionButton.Location = new System.Drawing.Point(149, 58);
      this._googleTypeRadionButton.Margin = new System.Windows.Forms.Padding(4);
      this._googleTypeRadionButton.Name = "_googleTypeRadionButton";
      this._googleTypeRadionButton.Size = new System.Drawing.Size(75, 21);
      this._googleTypeRadionButton.TabIndex = 1;
      this._googleTypeRadionButton.Text = "Google";
      this._googleTypeRadionButton.UseVisualStyleBackColor = true;
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(374, 243);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 5;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(266, 243);
      this._okButton.Margin = new System.Windows.Forms.Padding(4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 6;
      this._okButton.Text = "Ok";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _fruuxTypeRadioButton
      // 
      this._fruuxTypeRadioButton.AutoSize = true;
      this._fruuxTypeRadioButton.Location = new System.Drawing.Point(149, 112);
      this._fruuxTypeRadioButton.Margin = new System.Windows.Forms.Padding(4);
      this._fruuxTypeRadioButton.Name = "_fruuxTypeRadioButton";
      this._fruuxTypeRadioButton.Size = new System.Drawing.Size(64, 21);
      this._fruuxTypeRadioButton.TabIndex = 2;
      this._fruuxTypeRadioButton.Text = "Fruux";
      this._fruuxTypeRadioButton.UseVisualStyleBackColor = true;
      // 
      // _posteoTypeRadioButton
      // 
      this._posteoTypeRadioButton.AutoSize = true;
      this._posteoTypeRadioButton.Location = new System.Drawing.Point(149, 165);
      this._posteoTypeRadioButton.Margin = new System.Windows.Forms.Padding(4);
      this._posteoTypeRadioButton.Name = "_posteoTypeRadioButton";
      this._posteoTypeRadioButton.Size = new System.Drawing.Size(73, 21);
      this._posteoTypeRadioButton.TabIndex = 3;
      this._posteoTypeRadioButton.Text = "Posteo";
      this._posteoTypeRadioButton.UseVisualStyleBackColor = true;
      // 
      // _logoFruuxPictureBox
      // 
      this._logoFruuxPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._logoFruuxPictureBox.Location = new System.Drawing.Point(7, 102);
      this._logoFruuxPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this._logoFruuxPictureBox.Name = "_logoFruuxPictureBox";
      this._logoFruuxPictureBox.Size = new System.Drawing.Size(136, 40);
      this._logoFruuxPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._logoFruuxPictureBox.TabIndex = 8;
      this._logoFruuxPictureBox.TabStop = false;
      // 
      // _logoGooglePictureBox
      // 
      this._logoGooglePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._logoGooglePictureBox.Location = new System.Drawing.Point(7, 48);
      this._logoGooglePictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this._logoGooglePictureBox.Name = "_logoGooglePictureBox";
      this._logoGooglePictureBox.Size = new System.Drawing.Size(136, 40);
      this._logoGooglePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._logoGooglePictureBox.TabIndex = 9;
      this._logoGooglePictureBox.TabStop = false;
      // 
      // _logoPosteoPictureBox
      // 
      this._logoPosteoPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._logoPosteoPictureBox.Location = new System.Drawing.Point(7, 156);
      this._logoPosteoPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this._logoPosteoPictureBox.Name = "_logoPosteoPictureBox";
      this._logoPosteoPictureBox.Size = new System.Drawing.Size(136, 40);
      this._logoPosteoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this._logoPosteoPictureBox.TabIndex = 10;
      this._logoPosteoPictureBox.TabStop = false;
      // 
      // SelectOptionsDisplayTypeForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(490, 286);
      this.Controls.Add(this._logoPosteoPictureBox);
      this.Controls.Add(this._logoGooglePictureBox);
      this.Controls.Add(this._logoFruuxPictureBox);
      this.Controls.Add(this._posteoTypeRadioButton);
      this.Controls.Add(this._fruuxTypeRadioButton);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._cancelButton);
      this.Controls.Add(this._googleTypeRadionButton);
      this.Controls.Add(this._genericTypeRadioButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "SelectOptionsDisplayTypeForm";
      this.ShowIcon = false;
      this.Text = "Select Profile Type";
      ((System.ComponentModel.ISupportInitialize)(this._logoFruuxPictureBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this._logoGooglePictureBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this._logoPosteoPictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton _genericTypeRadioButton;
    private System.Windows.Forms.RadioButton _googleTypeRadionButton;
    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.RadioButton _fruuxTypeRadioButton;
    private System.Windows.Forms.RadioButton _posteoTypeRadioButton;
    private System.Windows.Forms.PictureBox _logoFruuxPictureBox;
    private System.Windows.Forms.PictureBox _logoGooglePictureBox;
    private System.Windows.Forms.PictureBox _logoPosteoPictureBox;
  }
}