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

        private bool slideShowRunning = false;

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
            this.server.ClientRequest += server_ClientRequest;
            Application.SlideShowNextSlide += Application_SlideShowOnNext;
            Application.SlideShowOnNext += Application_SlideShowOnNext;
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

        #region Server Events
        private void server_ClientRequest(object sender, ClientRequestEventArgs e)
        {
            switch ( e.Request )
            {
                case ClientRequest.StartPresentation:
                    this.StartPresentation();
                    break;
                case ClientRequest.StopPresentation:
                    this.StopPresentation();
                    break;
                case ClientRequest.NextSlide:
                    this.NextSlide();
                    break;
                case ClientRequest.PreviousSlide:
                    this.PreviousSlide();
                    break;
            }

            this.SendSlideData();
        }
        #endregion

        #region Slide Events & Interaction
        private void Application_SlideShowOnNext(Microsoft.Office.Interop.PowerPoint.SlideShowWindow Wn)
        {
            throw new NotImplementedException();
        }

        private void StartPresentation()
        {
            if ( !this.slideShowRunning )
            {
                this.slideShowRunning = true;

                Application.ActivePresentation.SlideShowSettings.Run();
            }
        }
        private void StopPresentation()
        {
            // workaround
            dynamic currentSlide = Application.ActiveWindow.View.Slide;
            int slideCount = Application.ActivePresentation.Slides.Count;
            Application.ActiveWindow.View.GotoSlide(slideCount);
            this.NextSlide();
            this.NextSlide();

            //throw new NotImplementedException("Stop presentation.");
            this.slideShowRunning = false;
        }
        private void NextSlide()
        {
            this.StartPresentation();
            Application.ActivePresentation.SlideShowWindow.View.Next();
        }
        private void PreviousSlide()
        {
            this.StartPresentation();
            Application.ActivePresentation.SlideShowWindow.View.Previous();
        }

        private void SendSlideData()
        {
            // throw new NotImplementedException("Sending slide data to client.");
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
