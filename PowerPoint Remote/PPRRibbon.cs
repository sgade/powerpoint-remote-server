using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using PowerPoint_Remote.Server;

namespace PowerPoint_Remote
{
    public partial class PPRRibbon
    {
        private PPRAddIn addInInstance = null;

        #region Ribbon Events
        private void PPRRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            this.addInInstance = PPRAddIn.GetInstance();
            PPRServer server = this.addInInstance.GetServer();
            if ( server != null )
            {
                server.Started += server_Started;
                server.Stopped += server_Stopped;
            }

            this.SetUIState(false);
        }
        #endregion

        #region UI Events
        private void buttonStartServer_Click(object sender, RibbonControlEventArgs e)
        {
            this.addInInstance.StartServer();
        }

        private void buttonStopServer_Click(object sender, RibbonControlEventArgs e)
        {
            this.addInInstance.StopServer();
        }
        #endregion

        #region Server Events
        private void server_Started(object sender, EventArgs e)
        {
            this.SetUIState(true);
        }

        private void server_Stopped(object sender, EventArgs e)
        {
            this.SetUIState(false);
        }
        #endregion

        #region UI Methods
        private void SetUIState(bool isServerRunning)
        {
            this.buttonStartServer.Enabled = !isServerRunning;
            this.buttonStopServer.Enabled = isServerRunning;
        }
        #endregion
    }
}
