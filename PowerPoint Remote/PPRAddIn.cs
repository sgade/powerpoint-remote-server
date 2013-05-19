using System;
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
        public PPRServer GetServer()
        {
            return this.server;
        }

        #region AddIn Events
        /// <summary>
        /// Called when the AddIn was loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // save our instance
            if ( PPRAddIn.instance == null )
                PPRAddIn.instance = this;

            this.server = new PPRServer();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception) e.ExceptionObject;
            String title = String.Format("{0} - {1}", Constants.NAME, "Unhandled Exception");

            System.Windows.Forms.MessageBox.Show(ex.ToString(), title);
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // clean on shutdown
            this.StopServer();
        }
        #endregion

        #region Public Methods
        public void StartServer()
        {
            String presentationName = Application.ActivePresentation.Name;
            this.server.Start(presentationName);
        }

        public void StopServer()
        {
            this.server.Stop();
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
