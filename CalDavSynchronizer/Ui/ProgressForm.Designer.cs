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
      this.SuspendLayout();
      // 
      // _progressBar
      // 
      this._progressBar.Location = new System.Drawing.Point(12, 12);
      this._progressBar.Name = "_progressBar";
      this._progressBar.Size = new System.Drawing.Size(319, 23);
      this._progressBar.TabIndex = 0;
      // 
      // ProgressForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(343, 56);
      this.ControlBox = false;
      this.Controls.Add(this._progressBar);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "ProgressForm";
      this.ShowIcon = false;
      this.Text = "Synchronizing...";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ProgressBar _progressBar;
  }
}