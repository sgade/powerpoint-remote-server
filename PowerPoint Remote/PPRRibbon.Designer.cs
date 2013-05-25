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
            this.groupServerControl = this.Factory.CreateRibbonGroup();
            this.groupPairingCode = this.Factory.CreateRibbonGroup();
            this.labelPairingCode = this.Factory.CreateRibbonLabel();
            this.groupConnectedDevice = this.Factory.CreateRibbonGroup();
            this.labelConnectedDevice = this.Factory.CreateRibbonLabel();
            this.buttonStartServer = this.Factory.CreateRibbonButton();
            this.buttonStopServer = this.Factory.CreateRibbonButton();
            this.buttonCopyCodeToClipboard = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.groupServerControl.SuspendLayout();
            this.groupPairingCode.SuspendLayout();
            this.groupConnectedDevice.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.groupServerControl);
            this.tab1.Groups.Add(this.groupPairingCode);
            this.tab1.Groups.Add(this.groupConnectedDevice);
            this.tab1.Label = "PowerPoint Remote";
            this.tab1.Name = "tab1";
            // 
            // groupServerControl
            // 
            this.groupServerControl.Items.Add(this.buttonStartServer);
            this.groupServerControl.Items.Add(this.buttonStopServer);
            this.groupServerControl.Label = "Server";
            this.groupServerControl.Name = "groupServerControl";
            // 
            // groupPairingCode
            // 
            this.groupPairingCode.Items.Add(this.labelPairingCode);
            this.groupPairingCode.Items.Add(this.buttonCopyCodeToClipboard);
            this.groupPairingCode.Label = "Pairing code";
            this.groupPairingCode.Name = "groupPairingCode";
            // 
            // labelPairingCode
            // 
            this.labelPairingCode.Label = "000000";
            this.labelPairingCode.Name = "labelPairingCode";
            // 
            // groupConnectedDevice
            // 
            this.groupConnectedDevice.Items.Add(this.labelConnectedDevice);
            this.groupConnectedDevice.Label = "Connected device";
            this.groupConnectedDevice.Name = "groupConnectedDevice";
            // 
            // labelConnectedDevice
            // 
            this.labelConnectedDevice.Label = "Not connected";
            this.labelConnectedDevice.Name = "labelConnectedDevice";
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
            // buttonCopyCodeToClipboard
            // 
            this.buttonCopyCodeToClipboard.Label = "Copy to clipboard";
            this.buttonCopyCodeToClipboard.Name = "buttonCopyCodeToClipboard";
            this.buttonCopyCodeToClipboard.ShowImage = true;
            this.buttonCopyCodeToClipboard.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonCopyCode_Click);
            // 
            // PPRRibbon
            // 
            this.Name = "PPRRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.PPRRibbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.groupServerControl.ResumeLayout(false);
            this.groupServerControl.PerformLayout();
            this.groupPairingCode.ResumeLayout(false);
            this.groupPairingCode.PerformLayout();
            this.groupConnectedDevice.ResumeLayout(false);
            this.groupConnectedDevice.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupServerControl;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonStartServer;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonStopServer;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupPairingCode;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel labelPairingCode;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonCopyCodeToClipboard;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupConnectedDevice;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel labelConnectedDevice;
    }

    partial class ThisRibbonCollection
    {
        internal PPRRibbon PPRRibbon
        {
            get { return this.GetRibbon<PPRRibbon>(); }
        }
    }
}
