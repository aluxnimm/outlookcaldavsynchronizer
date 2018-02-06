using System;
using System.Windows.Forms;
using CalDavSynchronizer.Globalization;

namespace CalDavSynchronizer.Ui.Options
{
  partial class SelectResourceForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectResourceForm));
      this._calendarDataGridView = new System.Windows.Forms.DataGridView();
      this.buttonCancel = new System.Windows.Forms.Button();
      this._addressBookDataGridView = new System.Windows.Forms.DataGridView();
      this._mainTab = new System.Windows.Forms.TabControl();
      this._calendarPage = new System.Windows.Forms.TabPage();
      this._addressBookPage = new System.Windows.Forms.TabPage();
      this._tasksPage = new System.Windows.Forms.TabPage();
      this._tasksDataGridView = new System.Windows.Forms.DataGridView();
      this.OkButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this._calendarDataGridView)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this._addressBookDataGridView)).BeginInit();
      this._mainTab.SuspendLayout();
      this._calendarPage.SuspendLayout();
      this._addressBookPage.SuspendLayout();
      this._tasksPage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this._tasksDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // _calendarDataGridView
      // 
      this._calendarDataGridView.AllowUserToAddRows = false;
      this._calendarDataGridView.AllowUserToDeleteRows = false;
      this._calendarDataGridView.AllowUserToResizeRows = false;
      this._calendarDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this._calendarDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this._calendarDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this._calendarDataGridView.Location = new System.Drawing.Point(3, 3);
      this._calendarDataGridView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this._calendarDataGridView.MultiSelect = false;
      this._calendarDataGridView.Name = "_calendarDataGridView";
      this._calendarDataGridView.ReadOnly = true;
      this._calendarDataGridView.RowHeadersVisible = false;
      this._calendarDataGridView.RowTemplate.Height = 24;
      this._calendarDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this._calendarDataGridView.Size = new System.Drawing.Size(721, 306);
      this._calendarDataGridView.TabIndex = 0;
      this._calendarDataGridView.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._calendarDataGridView_CellContentDoubleClick);
      this._calendarDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this._calendarDataGridView_CellFormatting);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(672, 350);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // _addressBookDataGridView
      // 
      this._addressBookDataGridView.AllowUserToAddRows = false;
      this._addressBookDataGridView.AllowUserToDeleteRows = false;
      this._addressBookDataGridView.AllowUserToResizeRows = false;
      this._addressBookDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this._addressBookDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this._addressBookDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this._addressBookDataGridView.Location = new System.Drawing.Point(3, 3);
      this._addressBookDataGridView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this._addressBookDataGridView.MultiSelect = false;
      this._addressBookDataGridView.Name = "_addressBookDataGridView";
      this._addressBookDataGridView.ReadOnly = true;
      this._addressBookDataGridView.RowHeadersVisible = false;
      this._addressBookDataGridView.RowTemplate.Height = 24;
      this._addressBookDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this._addressBookDataGridView.Size = new System.Drawing.Size(721, 306);
      this._addressBookDataGridView.TabIndex = 3;
      this._addressBookDataGridView.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._addressBookDataGridView_CellContentDoubleClick);
      // 
      // _mainTab
      // 
      this._mainTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._mainTab.Controls.Add(this._calendarPage);
      this._mainTab.Controls.Add(this._addressBookPage);
      this._mainTab.Controls.Add(this._tasksPage);
      this._mainTab.Location = new System.Drawing.Point(12, 6);
      this._mainTab.Name = "_mainTab";
      this._mainTab.SelectedIndex = 0;
      this._mainTab.Size = new System.Drawing.Size(735, 338);
      this._mainTab.TabIndex = 4;
      // 
      // _calendarPage
      // 
      this._calendarPage.Controls.Add(this._calendarDataGridView);
      this._calendarPage.Location = new System.Drawing.Point(4, 22);
      this._calendarPage.Name = "_calendarPage";
      this._calendarPage.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this._calendarPage.Size = new System.Drawing.Size(727, 312);
      this._calendarPage.TabIndex = 0;
      this._calendarPage.Text = "Calendars";
      this._calendarPage.UseVisualStyleBackColor = true;
      // 
      // _addressBookPage
      // 
      this._addressBookPage.Controls.Add(this._addressBookDataGridView);
      this._addressBookPage.Location = new System.Drawing.Point(4, 22);
      this._addressBookPage.Name = "_addressBookPage";
      this._addressBookPage.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this._addressBookPage.Size = new System.Drawing.Size(727, 312);
      this._addressBookPage.TabIndex = 1;
      this._addressBookPage.Text = "Address Books";
      this._addressBookPage.UseVisualStyleBackColor = true;
      // 
      // _tasksPage
      // 
      this._tasksPage.Controls.Add(this._tasksDataGridView);
      this._tasksPage.Location = new System.Drawing.Point(4, 22);
      this._tasksPage.Name = "_tasksPage";
      this._tasksPage.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this._tasksPage.Size = new System.Drawing.Size(727, 312);
      this._tasksPage.TabIndex = 2;
      this._tasksPage.Text = "Tasks";
      this._tasksPage.UseVisualStyleBackColor = true;
      // 
      // _tasksDataGridView
      // 
      this._tasksDataGridView.AllowUserToAddRows = false;
      this._tasksDataGridView.AllowUserToDeleteRows = false;
      this._tasksDataGridView.AllowUserToResizeRows = false;
      this._tasksDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this._tasksDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this._tasksDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this._tasksDataGridView.Location = new System.Drawing.Point(3, 3);
      this._tasksDataGridView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this._tasksDataGridView.MultiSelect = false;
      this._tasksDataGridView.Name = "_tasksDataGridView";
      this._tasksDataGridView.ReadOnly = true;
      this._tasksDataGridView.RowHeadersVisible = false;
      this._tasksDataGridView.RowTemplate.Height = 24;
      this._tasksDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this._tasksDataGridView.Size = new System.Drawing.Size(727, 312);
      this._tasksDataGridView.TabIndex = 4;
      this._tasksDataGridView.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._tasksDataGridView_CellContentDoubleClick);
      // 
      // OkButton
      // 
      this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.OkButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.OkButton.Location = new System.Drawing.Point(591, 350);
      this.OkButton.Name = "OkButton";
      this.OkButton.Size = new System.Drawing.Size(75, 23);
      this.OkButton.TabIndex = 5;
      this.OkButton.Text = "OK";
      this.OkButton.UseVisualStyleBackColor = true;
      this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
      // 
      // SelectResourceForm
      // 
      this.AcceptButton = this.OkButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(754, 379);
      this.Controls.Add(this.OkButton);
      this.Controls.Add(this._mainTab);
      this.Controls.Add(this.buttonCancel);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "SelectResourceForm";
      this.Text = "Select Resource";
      ((System.ComponentModel.ISupportInitialize)(this._calendarDataGridView)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this._addressBookDataGridView)).EndInit();
      this._mainTab.ResumeLayout(false);
      this._calendarPage.ResumeLayout(false);
      this._addressBookPage.ResumeLayout(false);
      this._tasksPage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this._tasksDataGridView)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.DataGridView _calendarDataGridView;
    private System.Windows.Forms.Button buttonCancel;
    private DataGridView _addressBookDataGridView;
    private TabControl _mainTab;
    private TabPage _calendarPage;
    private TabPage _addressBookPage;
    private Button OkButton;
    private TabPage _tasksPage;
    private DataGridView _tasksDataGridView;
  }
}