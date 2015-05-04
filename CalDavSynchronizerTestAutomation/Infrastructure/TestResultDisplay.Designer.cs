namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  partial class TestResultDisplay
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this._testsTreeView = new System.Windows.Forms.TreeView();
      this._detailsTextBox = new System.Windows.Forms.TextBox();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this._assertTrueButton = new System.Windows.Forms.Button();
      this._assertFalseButton = new System.Windows.Forms.Button();
      this._assertLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this._testsTreeView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this._detailsTextBox);
      this.splitContainer1.Size = new System.Drawing.Size(591, 308);
      this.splitContainer1.SplitterDistance = 197;
      this.splitContainer1.TabIndex = 0;
      // 
      // _testsTreeView
      // 
      this._testsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this._testsTreeView.Location = new System.Drawing.Point(0, 0);
      this._testsTreeView.Name = "_testsTreeView";
      this._testsTreeView.Size = new System.Drawing.Size(197, 308);
      this._testsTreeView.TabIndex = 0;
      // 
      // _detailsTextBox
      // 
      this._detailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this._detailsTextBox.Location = new System.Drawing.Point(0, 0);
      this._detailsTextBox.Multiline = true;
      this._detailsTextBox.Name = "_detailsTextBox";
      this._detailsTextBox.Size = new System.Drawing.Size(390, 308);
      this._detailsTextBox.TabIndex = 0;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this._assertLabel);
      this.splitContainer2.Panel2.Controls.Add(this._assertFalseButton);
      this.splitContainer2.Panel2.Controls.Add(this._assertTrueButton);
      this.splitContainer2.Size = new System.Drawing.Size(591, 617);
      this.splitContainer2.SplitterDistance = 308;
      this.splitContainer2.TabIndex = 1;
      // 
      // _assertTrueButton
      // 
      this._assertTrueButton.Location = new System.Drawing.Point(397, 176);
      this._assertTrueButton.Name = "_assertTrueButton";
      this._assertTrueButton.Size = new System.Drawing.Size(75, 23);
      this._assertTrueButton.TabIndex = 0;
      this._assertTrueButton.Text = "True";
      this._assertTrueButton.UseVisualStyleBackColor = true;
      this._assertTrueButton.Click += new System.EventHandler(this._assertTrueButton_Click);
      // 
      // _assertFalseButton
      // 
      this._assertFalseButton.Location = new System.Drawing.Point(316, 176);
      this._assertFalseButton.Name = "_assertFalseButton";
      this._assertFalseButton.Size = new System.Drawing.Size(75, 23);
      this._assertFalseButton.TabIndex = 1;
      this._assertFalseButton.Text = "False";
      this._assertFalseButton.UseVisualStyleBackColor = true;
      this._assertFalseButton.Click += new System.EventHandler(this._assertFalseButton_Click);
      // 
      // _assertLabel
      // 
      this._assertLabel.AutoSize = true;
      this._assertLabel.Location = new System.Drawing.Point(55, 53);
      this._assertLabel.Name = "_assertLabel";
      this._assertLabel.Size = new System.Drawing.Size(35, 13);
      this._assertLabel.TabIndex = 2;
      this._assertLabel.Text = "label1";
      // 
      // TestResultDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(591, 617);
      this.Controls.Add(this.splitContainer2);
      this.Name = "TestResultDisplay";
      this.Text = "CalDavSynchronizer Tests";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TreeView _testsTreeView;
    private System.Windows.Forms.TextBox _detailsTextBox;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.Label _assertLabel;
    private System.Windows.Forms.Button _assertFalseButton;
    private System.Windows.Forms.Button _assertTrueButton;
  }
}