using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using PowerPoint_Remote.Server;

namespace PowerPoint_Remote
{
    public partial class PPRAddIn
    {
        #region Global Static Getters
        private static PPRAddIn instance = null;
        public static PPRAddIn GetInstance()
        {
            return PPRAddIn.instance;
        }
        #endregion

        private PPRServer server = null;

        #region AddIn Events
        /// <summary>
        /// Called when the AddIn was loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // save our instance
            if ( PPRAddIn.instance == null )
                PPRAddIn.instance = this;

            this.server = new PPRServer();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // clean on shutdown
            this.StopServer();
        }
        #endregion

        #region Getters
        public bool IsServerRunning()
        {
            return this.server.isRunning();
        }
        #endregion

        #region Public Methods
        public bool StartServer()
        {
            this.server.Start();
            return this.IsServerRunning();
        }

        public bool StopServer()
        {
            this.server.Stop();
            return this.IsServerRunning();
        }
        #endregion

        #region Von VSTO generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
