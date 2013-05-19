using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
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
            // listen to presentation changes
            Application.SlideShowBegin += Application_SlideShowBegin;
            Application.SlideShowEnd += Application_SlideShowEnd;
            Application.SlideShowNextSlide += Application_SlideShowNextSlide;
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
        }
        #endregion

        #region Slide Events & Interaction
        void Application_SlideShowBegin(SlideShowWindow Wn)
        {
            this.slideShowRunning = true;
        }
        private void Application_SlideShowEnd(Presentation Pres)
        {
            this.slideShowRunning = false;
        }
        private void Application_SlideShowNextSlide(Microsoft.Office.Interop.PowerPoint.SlideShowWindow Wn)
        {
            // slide changed, send data
            this.SendSlideData();
        }

        private void StartPresentation()
        {
            if ( !this.slideShowRunning )
                Application.ActivePresentation.SlideShowSettings.Run();
        }
        private void StopPresentation()
        {
            if ( this.slideShowRunning )
            {
                // TODO implements
                throw new NotImplementedException("Method unknown.");
            }
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
            if ( this.slideShowRunning )
            {
                Slide currentSlide = Application.ActivePresentation.SlideShowWindow.View.Slide;

                this.SendSlideNotes(currentSlide);
                this.SendSlideImageData(currentSlide);
            }
        }
        private void SendSlideNotes(Slide slide)
        {
            String notes = this.GetSlideNotes(slide);
            Debug.WriteLineIf(notes != null, "Notes: " + notes);
            if ( notes != null )
                this.server.SendSlideNotes(notes);
        }
        private String GetSlideNotes(Slide slide)
        {
            String notes = null;

            notes = slide.NotesPage.Shapes[2].TextFrame.TextRange.Text;

            return notes;
        }
        private void SendSlideImageData(Slide slide)
        {
            byte[] slideData = this.GetSlideByteArray(slide);
            this.server.SendSlideImageData(slideData);
        }
        private byte[] GetSlideByteArray(Slide slide)
        {
            String filename = Path.GetTempFileName();

            slide.Export(filename, "PNG");

            byte[] buffer = File.ReadAllBytes(filename);
            File.Delete(filename);
            return buffer;
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
