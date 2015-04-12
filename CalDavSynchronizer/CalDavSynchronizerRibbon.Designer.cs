namespace CalDavSynchronizer
{
  partial class CalDavSynchronizerRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    public CalDavSynchronizerRibbon ()
      : base (Globals.Factory.GetRibbonFactory ())
    {
      InitializeComponent ();
    }

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
      this.tab1 = this.Factory.CreateRibbonTab();
      this.group1 = this.Factory.CreateRibbonGroup();
      this.SynchronizeNowButton = this.Factory.CreateRibbonButton();
      this.OptionsButton = this.Factory.CreateRibbonButton();
      this.AboutButton = this.Factory.CreateRibbonButton();
      this.tab1.SuspendLayout();
      this.group1.SuspendLayout();
      // 
      // tab1
      // 
      this.tab1.Groups.Add(this.group1);
      this.tab1.Label = "CalDav Synchronizer";
      this.tab1.Name = "tab1";
      // 
      // group1
      // 
      this.group1.Items.Add(this.SynchronizeNowButton);
      this.group1.Items.Add(this.OptionsButton);
      this.group1.Items.Add(this.AboutButton);
      this.group1.Name = "group1";
      // 
      // SynchronizeNowButton
      // 
      this.SynchronizeNowButton.Image = global::CalDavSynchronizer.Properties.Resources.Sync;
      this.SynchronizeNowButton.Label = "Synchronize now";
      this.SynchronizeNowButton.Name = "SynchronizeNowButton";
      this.SynchronizeNowButton.ShowImage = true;
      this.SynchronizeNowButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.SynchronizeNowButton_Click);
      // 
      // OptionsButton
      // 
      this.OptionsButton.Image = global::CalDavSynchronizer.Properties.Resources.Options;
      this.OptionsButton.Label = "Options";
      this.OptionsButton.Name = "OptionsButton";
      this.OptionsButton.ShowImage = true;
      this.OptionsButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OptionsButton_Click);
      // 
      // AboutButton
      // 
      this.AboutButton.Image = global::CalDavSynchronizer.Properties.Resources.About;
      this.AboutButton.Label = "About";
      this.AboutButton.Name = "AboutButton";
      this.AboutButton.ShowImage = true;
      // 
      // CalDavSynchronizerRibbon
      // 
      this.Name = "CalDavSynchronizerRibbon";
      this.RibbonType = "Microsoft.Outlook.Explorer";
      this.Tabs.Add(this.tab1);
      this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.CalDavSynchronizerRibbon_Load);
      this.tab1.ResumeLayout(false);
      this.tab1.PerformLayout();
      this.group1.ResumeLayout(false);
      this.group1.PerformLayout();

    }

    #endregion

    internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
    internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton SynchronizeNowButton;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton OptionsButton;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton AboutButton;
  }

  partial class ThisRibbonCollection
  {
    internal CalDavSynchronizerRibbon CalDavSynchronizerRibbon
    {
      get { return this.GetRibbon<CalDavSynchronizerRibbon> (); }
    }
  }
}
