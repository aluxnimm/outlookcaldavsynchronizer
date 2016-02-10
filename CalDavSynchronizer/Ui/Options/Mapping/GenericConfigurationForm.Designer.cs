using System;

namespace CalDavSynchronizer.Ui.Options.Mapping
{
  partial class GenericConfigurationForm<TElement>
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
      this._okButton = new System.Windows.Forms.Button();
      this._contentTextBox = new System.Windows.Forms.RichTextBox();
      this._cancelButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(528, 459);
      this._okButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 0;
      this._okButton.Text = "Ok";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this._okButton_Click);
      // 
      // _contentTextBox
      // 
      this._contentTextBox.AcceptsTab = true;
      this._contentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._contentTextBox.Location = new System.Drawing.Point(17, 16);
      this._contentTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._contentTextBox.Name = "_contentTextBox";
      this._contentTextBox.Size = new System.Drawing.Size(717, 435);
      this._contentTextBox.TabIndex = 1;
      this._contentTextBox.Text = "";
      this._contentTextBox.WordWrap = false;
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(636, 459);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 2;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // GenericConfigurationForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(752, 502);
      this.Controls.Add(this._cancelButton);
      this.Controls.Add(this._contentTextBox);
      this.Controls.Add(this._okButton);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "GenericConfigurationForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "EditConfiguration";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.RichTextBox _contentTextBox;
    private System.Windows.Forms.Button _cancelButton;
  }
}