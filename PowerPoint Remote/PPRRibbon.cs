using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

namespace PowerPoint_Remote
{
    public partial class PPRRibbon
    {
        private PPRAddIn addInInstance = null;

        #region Ribbon Events
        private void PPRRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            this.addInInstance = PPRAddIn.GetInstance();

            this.SetUIState();
        }
        #endregion

        #region UI Events
        private void buttonStartServer_Click(object sender, RibbonControlEventArgs e)
        {
            if ( this.addInInstance.StartServer() )
                this.SetUIState();
        }

        private void buttonStopServer_Click(object sender, RibbonControlEventArgs e)
        {
            if ( this.addInInstance.StopServer() )
                this.SetUIState();
        }
        #endregion

        #region UI Methods
        private void SetUIState()
        {
            bool serverRunning = this.addInInstance.IsServerRunning();

            this.buttonStartServer.Enabled = !serverRunning;
            this.buttonStopServer.Enabled = serverRunning;
        }
        #endregion
    }
}
