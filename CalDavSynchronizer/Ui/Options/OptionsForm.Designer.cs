using System;
using System.Windows.Forms;

namespace CalDavSynchronizer.Ui.Options
{
  partial class OptionsForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
      this._okButton = new System.Windows.Forms.Button();
      this._cancelButton = new System.Windows.Forms.Button();
      this._tabImageList = new System.Windows.Forms.ImageList(this.components);
      this._addProfileButton = new System.Windows.Forms.Button();
      this._tabControl = new CalDavSynchronizer.Ui.DraggableTabControl();
      this.SuspendLayout();
      // 
      // _okButton
      // 
      this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._okButton.Location = new System.Drawing.Point(385, 737);
      this._okButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(100, 28);
      this._okButton.TabIndex = 40;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this.OkButton_Click);
      // 
      // _cancelButton
      // 
      this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(493, 737);
      this._cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(100, 28);
      this._cancelButton.TabIndex = 41;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      // 
      // _tabImageList
      // 
      this._tabImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_tabImageList.ImageStream")));
      this._tabImageList.TransparentColor = System.Drawing.Color.Transparent;
      this._tabImageList.Images.SetKeyName(0, "inactive");
      this._tabImageList.Images.SetKeyName(1, "Appointment");
      this._tabImageList.Images.SetKeyName(2, "AppointmentDisabled");
      this._tabImageList.Images.SetKeyName(3, "Task");
      this._tabImageList.Images.SetKeyName(4, "TaskDisabled");
      this._tabImageList.Images.SetKeyName(5, "Contact");
      this._tabImageList.Images.SetKeyName(6, "ContactDisabled");
      // 
      // _addProfileButton
      // 
      this._addProfileButton.Location = new System.Drawing.Point(17, 737);
      this._addProfileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this._addProfileButton.Name = "_addProfileButton";
      this._addProfileButton.Size = new System.Drawing.Size(100, 28);
      this._addProfileButton.TabIndex = 43;
      this._addProfileButton.Text = "Add";
      this._addProfileButton.UseVisualStyleBackColor = true;
      this._addProfileButton.Click += new System.EventHandler(this._addProfileButton_Click);
      // 
      // _tabControl
      // 
      this._tabControl.AllowDrop = true;
      this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._tabControl.ImageList = this._tabImageList;
      this._tabControl.Location = new System.Drawing.Point(17, 16);
      this._tabControl.Margin = new System.Windows.Forms.Padding(4);
      this._tabControl.Name = "_tabControl";
      this._tabControl.SelectedIndex = 0;
      this._tabControl.Size = new System.Drawing.Size(576, 709);
      this._tabControl.TabIndex = 2;
      // 
      // OptionsForm
      // 
      this.AcceptButton = this._okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this._cancelButton;
      this.ClientSize = new System.Drawing.Size(609, 782);
      this.ControlBox = false;
      this.Controls.Add(this._addProfileButton);
      this.Controls.Add(this._tabControl);
      this.Controls.Add(this._cancelButton);
      this.Controls.Add(this._okButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "OptionsForm";
      this.Text = "CalDav Synchronizer";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.Button _cancelButton;
    private DraggableTabControl _tabControl;
    private System.Windows.Forms.Button _addProfileButton;
    private System.Windows.Forms.ImageList _tabImageList;

  }
}