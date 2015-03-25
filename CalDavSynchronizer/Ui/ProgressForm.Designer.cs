namespace CalDavSynchronizer.Ui
{
  partial class ProgressForm
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
      this._progressBar = new System.Windows.Forms.ProgressBar();
      this._messageLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // _progressBar
      // 
      this._progressBar.Location = new System.Drawing.Point(12, 12);
      this._progressBar.Name = "_progressBar";
      this._progressBar.Size = new System.Drawing.Size(319, 23);
      this._progressBar.TabIndex = 0;
      // 
      // _messageLabel
      // 
      this._messageLabel.AutoSize = true;
      this._messageLabel.Location = new System.Drawing.Point(12, 44);
      this._messageLabel.Name = "_messageLabel";
      this._messageLabel.Size = new System.Drawing.Size(0, 13);
      this._messageLabel.TabIndex = 1;
      // 
      // ProgressForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(343, 66);
      this.ControlBox = false;
      this.Controls.Add(this._messageLabel);
      this.Controls.Add(this._progressBar);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "ProgressForm";
      this.ShowIcon = false;
      this.Text = "CalDavSynchronizer - Synchronizing...";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar _progressBar;
    private System.Windows.Forms.Label _messageLabel;
  }
}