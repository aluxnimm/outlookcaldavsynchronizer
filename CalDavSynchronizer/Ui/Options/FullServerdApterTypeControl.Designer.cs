using System;

namespace CalDavSynchronizer.Ui.Options
{
  partial class FullServerdApterTypeControl
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
      this._serverAdapterTypeComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.SuspendLayout();
      // 
      // _serverAdapterTypeComboBox
      // 
      this._serverAdapterTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._serverAdapterTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._serverAdapterTypeComboBox.FormattingEnabled = true;
      this._serverAdapterTypeComboBox.Location = new System.Drawing.Point(89, 0);
      this._serverAdapterTypeComboBox.Name = "_serverAdapterTypeComboBox";
      this._serverAdapterTypeComboBox.Size = new System.Drawing.Size(211, 21);
      this._serverAdapterTypeComboBox.TabIndex = 20;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 13);
      this.label1.TabIndex = 19;
      this.label1.Text = "Server adapter:";
      // 
      // FullServerdApterTypeControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this._serverAdapterTypeComboBox);
      this.Controls.Add(this.label1);
      this.Name = "FullServerdApterTypeControl";
      this.Size = new System.Drawing.Size(300, 21);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.ComboBox _serverAdapterTypeComboBox;
    private System.Windows.Forms.Label label1;
  }
}
