namespace PowerPoint_Remote
{
    partial class PPRRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public PPRRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">"true", wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls "false".</param>
        protected override void Dispose(bool disposing)
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für Designerunterstützung -
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.buttonStartServer = this.Factory.CreateRibbonButton();
            this.buttonStopServer = this.Factory.CreateRibbonButton();
            this.group2 = this.Factory.CreateRibbonGroup();
            this.labelName = this.Factory.CreateRibbonLabel();
            this.tab1.SuspendLayout();
            this.group1.SuspendLayout();
            this.group2.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Groups.Add(this.group2);
            this.tab1.Label = "PowerPoint Remote";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Items.Add(this.buttonStartServer);
            this.group1.Items.Add(this.buttonStopServer);
            this.group1.Label = "Server";
            this.group1.Name = "group1";
            // 
            // buttonStartServer
            // 
            this.buttonStartServer.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonStartServer.KeyTip = "R";
            this.buttonStartServer.Label = "Start";
            this.buttonStartServer.Name = "buttonStartServer";
            this.buttonStartServer.OfficeImageId = "OutlookGlobe";
            this.buttonStartServer.ShowImage = true;
            this.buttonStartServer.SuperTip = "Starts the server";
            this.buttonStartServer.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonStartServer_Click);
            // 
            // buttonStopServer
            // 
            this.buttonStopServer.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonStopServer.Enabled = false;
            this.buttonStopServer.KeyTip = "S";
            this.buttonStopServer.Label = "Stop";
            this.buttonStopServer.Name = "buttonStopServer";
            this.buttonStopServer.OfficeImageId = "PrintPreviewClose";
            this.buttonStopServer.ShowImage = true;
            this.buttonStopServer.SuperTip = "Stops the server.";
            this.buttonStopServer.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonStopServer_Click);
            // 
            // group2
            // 
            this.group2.Items.Add(this.labelName);
            this.group2.Label = "Connected device";
            this.group2.Name = "group2";
            // 
            // labelName
            // 
            this.labelName.Label = "No connection";
            this.labelName.Name = "labelName";
            // 
            // PPRRibbon
            // 
            this.Name = "PPRRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.PPRRibbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonStartServer;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonStopServer;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel labelName;
    }

    partial class ThisRibbonCollection
    {
        internal PPRRibbon PPRRibbon
        {
            get { return this.GetRibbon<PPRRibbon>(); }
        }
    }
}
