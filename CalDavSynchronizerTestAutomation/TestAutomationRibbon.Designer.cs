namespace CalDavSynchronizerTestAutomation
{
  partial class CalDavSynchronizerTestRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    public CalDavSynchronizerTestRibbon ()
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
      this.StartTestsButton = this.Factory.CreateRibbonButton();
      this.StartTestsExcludeManualButton = this.Factory.CreateRibbonButton();
      this.ImportIcsData = this.Factory.CreateRibbonButton();
      this.tab1.SuspendLayout();
      this.group1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tab1
      // 
      this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
      this.tab1.Groups.Add(this.group1);
      this.tab1.Label = "CalDavSynchronizerTests";
      this.tab1.Name = "tab1";
      // 
      // group1
      // 
      this.group1.Items.Add(this.StartTestsButton);
      this.group1.Items.Add(this.StartTestsExcludeManualButton);
      this.group1.Items.Add(this.ImportIcsData);
      this.group1.Name = "group1";
      // 
      // StartTestsButton
      // 
      this.StartTestsButton.Label = "Start tests (all)";
      this.StartTestsButton.Name = "StartTestsButton";
      this.StartTestsButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.StartTestsButton_Click);
      // 
      // StartTestsExcludeManualButton
      // 
      this.StartTestsExcludeManualButton.Label = "Start tests (exclude manual)";
      this.StartTestsExcludeManualButton.Name = "StartTestsExcludeManualButton";
      this.StartTestsExcludeManualButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.StartTestsExcludeManualButton_Click);
      // 
      // ImportIcsData
      // 
      this.ImportIcsData.Label = "Import ICS Data";
      this.ImportIcsData.Name = "ImportIcsData";
      this.ImportIcsData.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ImportIcsData_Click);
      // 
      // CalDavSynchronizerTestRibbon
      // 
      this.Name = "CalDavSynchronizerTestRibbon";
      this.RibbonType = "Microsoft.Outlook.Explorer";
      this.Tabs.Add(this.tab1);
      this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.TestAutomationRibbon_Load);
      this.tab1.ResumeLayout(false);
      this.tab1.PerformLayout();
      this.group1.ResumeLayout(false);
      this.group1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
    internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton StartTestsButton;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton StartTestsExcludeManualButton;
    internal Microsoft.Office.Tools.Ribbon.RibbonButton ImportIcsData;
  }

  partial class ThisRibbonCollection
  {
    internal CalDavSynchronizerTestRibbon TestAutomationRibbon
    {
      get { return this.GetRibbon<CalDavSynchronizerTestRibbon> (); }
    }
  }
}
