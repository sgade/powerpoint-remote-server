using System;
using System.Windows.Forms;
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

            this.server_Stopped(this, EventArgs.Empty);
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

        private void buttonCopyCode_Click(object sender, RibbonControlEventArgs e)
        {
            String code = this.labelPairingCode.Label;

            Clipboard.SetText(code);
        }
        #endregion

        #region Server Events
        private void server_Started(object sender, StartedEventArgs e)
        {
            this.SetUIState(true);
            this.SetPairingCode(e.PairingCode);
        }

        private void server_Stopped(object sender, EventArgs e)
        {
            this.SetUIState(false);
            this.SetPairingCode(null);
        }
        #endregion

        #region UI Methods
        private void SetUIState(bool isServerRunning)
        {
            this.buttonStartServer.Enabled = !isServerRunning;
            this.buttonStopServer.Enabled = isServerRunning;
        }
        private void SetPairingCode(String code)
        {
            this.labelPairingCode.Label = code;
        }
        #endregion

       
    }
}
