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
#if !DEBUG
            this.buttonCopyCodeToClipboard.Visible = false;
#endif

            this.addInInstance = PPRAddIn.GetInstance();
            PPRServer server = this.addInInstance.GetServer();
            if ( server != null )
            {
                server.Started += server_Started;
                server.Stopped += server_Stopped;

                server.ClientStatus += server_ClientStatus;
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
            String[] codeParts = code.Split(' ');

            code = "";
            for ( int i = 0; i < codeParts.Length; i++ )
            {
                code += codeParts[i];
            }

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

        private void server_ClientStatus(object sender, ClientStatusEventArgs e)
        {
            this.SetDeviceConnectionState(e.ClientConnected);
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
            if ( code != null )
            {
                String easyReadableCode = "";
                for ( int i = 0; i < code.Length; i++ )
                {
                    easyReadableCode += code[i] + " ";
                }
                this.labelPairingCode.Label = easyReadableCode;
            }
            else
                this.labelPairingCode.Label = "You need to start the remote server.";
        }

        private void SetDeviceConnectionState(bool clientConnected)
        {
            if ( clientConnected )
                this.labelConnectedDevice.Label = "Connected";
            else
                this.labelConnectedDevice.Label = "Not connected";
        }
        #endregion
    }
}
