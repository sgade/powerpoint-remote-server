using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.PowerPoint;
using PowerPoint_Remote.Server;

namespace PowerPoint_Remote
{
    /// <summary>
    /// The main PowerPointRemote class.
    /// This is the entry point into code execution.
    /// </summary>
    public partial class PPRAddIn
    {
        #region Global Static Getters
        /// <summary>
        /// The one and only instance of this class. (should be)
        /// </summary>
        private static PPRAddIn instance = null;
        /// <summary>
        /// Returns the instance reference to PPRAddIn. (may be null)
        /// </summary>
        /// <returns>The instance reference.</returns>
        public static PPRAddIn GetInstance()
        {
            return PPRAddIn.instance;
        }
        #endregion

        /// <summary>
        /// The server instance.
        /// </summary>
        private PPRServer server = null;
        /// <summary>
        /// Returns the instance reference to the PPRServer. (may be null)
        /// </summary>
        /// <returns>The instance reference.</returns>
        public PPRServer GetServer()
        {
            return this.server;
        }

        /// <summary>
        /// Indicates whether PowerPoint is currently running a slide show.
        /// </summary>
        private bool slideShowRunning = false;

        #region AddIn Events
        /// <summary>
        /// Called when the AddIn was loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // Add a handler for when we crash (hopefully: never)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // save our instance
            if ( PPRAddIn.instance == null )
                PPRAddIn.instance = this;

            // create a new server
            this.server = new PPRServer();
            // listen to requests (commands) issued by the connected peer
            this.server.ClientRequest += server_ClientRequest;
            // listen to presentation changes (inside PowerPoint)
            Application.SlideShowBegin += Application_SlideShowBegin;
            Application.SlideShowEnd += Application_SlideShowEnd;
            Application.SlideShowNextSlide += Application_SlideShowNextSlide;
        }

        /// <summary>
        /// Handler for when an unhandled exception was raised (i.e. not <code>catch</code>ed).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event args.</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // get the original exception
            Exception ex = (Exception) e.ExceptionObject;
            // format the message box title
            String title = String.Format("{0} - {1}", Constants.NAME, "Unhandled Exception");

#if DEBUG
            System.Windows.Forms.MessageBox.Show(ex.ToString(), title, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
#else
            System.Windows.Forms.MessageBox.Show(Constants.CRASH_ERRORTEXT, title, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
#endif
        }

        /// <summary>
        /// Called when the AddIn should be closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event args.</param>
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // clean on shutdown
            this.StopServer();
        }
        #endregion

        #region Server Events
        /// <summary>
        /// Event handler for when the remote clients sent a request for a command to be executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event args.</param>
        private void server_ClientRequest(object sender, ClientRequestEventArgs e)
        {
            // look at the command that should be fulfilled
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
        /// <summary>
        /// Event handler for when the slide show begun.
        /// </summary>
        /// <param name="Wn">The show window.</param>
        private void Application_SlideShowBegin(SlideShowWindow Wn)
        {
            // save state
            this.slideShowRunning = true;
        }
        /// <summary>
        /// Event handler for when the slide show ended.
        /// </summary>
        /// <param name="Pres">The presentation that had been shown.</param>
        private void Application_SlideShowEnd(Presentation Pres)
        {
            // save state
            this.slideShowRunning = false;
            // send out a notification to the client
            this.server.SendStop();
        }
        /// <summary>
        /// Event handler for when another slide is shown.
        /// </summary>
        /// <param name="Wn">The window.</param>
        private void Application_SlideShowNextSlide(SlideShowWindow Wn)
        {
            // slide changed, send data
            this.SendSlideData();
        }

        /// <summary>
        /// Starts the PowerPoint Presentation, if not already running.
        /// </summary>
        private void StartPresentation()
        {
            // check the current "running" status
            if ( !this.slideShowRunning )
            {
                try
                {
                    // start presentation
                    Application.ActivePresentation.SlideShowSettings.Run();
                }
                catch ( COMException )
                {
                    // Completely unknown error
                }
            }
        }
        /// <summary>
        /// Stops the PowerPoint Presentation, if running.
        /// </summary>
        private void StopPresentation()
        {
            // check the current "running" status
            if ( this.slideShowRunning )
            {
                try
                {
                    // stop presentation
                    Application.Quit(); // REALLY?!
                }
                catch ( COMException )
                {
                    // Completely unknown error
                }
            }
        }
        /// <summary>
        /// Switches PowerPoint to the next key point (e.g. animation or slide).
        /// </summary>
        private void NextSlide()
        {
            // make sure we are running a presentation
            this.StartPresentation();
            try
            {
                // next slide
                Application.ActivePresentation.SlideShowWindow.View.Next();
            }
            catch ( COMException )
            {
                // Completely unknown error
            }
        }
        /// <summary>
        /// Switches PowerPoint to the last key point (e.g. animation or slide).
        /// </summary>
        private void PreviousSlide()
        {
            // make sure we are running a presentation
            this.StartPresentation();
            try
            {
                // previous slide
                Application.ActivePresentation.SlideShowWindow.View.Previous();
            }
            catch ( COMException )
            {
                // Completely unknown error
            }
        }

        /// <summary>
        /// Sends all data of the current slide to the client.
        /// </summary>
        private void SendSlideData()
        {
            // if we have a presentation
            if ( this.slideShowRunning )
            {
                // the current slide object
                Slide currentSlide = Application.ActivePresentation.SlideShowWindow.View.Slide;

                // send notes first
                this.SendSlideNotes(currentSlide);
                // then the image datat
                this.SendSlideImageData(currentSlide);
            }
        }
        /// <summary>
        /// Sends the note data of the given slide to the client.
        /// </summary>
        /// <param name="slide">The <code>Slide</code>'s note to send.</param>
        private void SendSlideNotes(Slide slide)
        {
            // get notes
            String notes = this.GetSlideNotes(slide);
            // only send if we have some!
            if ( notes != null )
                this.server.SendSlideNotes(notes);
        }
        /// <summary>
        /// Gets the notes as a <code>String</code> from the given <code>Slide</code>.
        /// </summary>
        /// <param name="slide">The <code>Slide</code> to take the notes from.</param>
        /// <returns>The found notes.</returns>
        private String GetSlideNotes(Slide slide)
        {
            String notes = null;

            // get notes with this "thing"
            notes = slide.NotesPage.Shapes[2].TextFrame.TextRange.Text;
            // replace chars
            notes = notes.Replace('\r', '\n');

            return notes;
        }
        /// <summary>
        /// Sends the image data of the given <code>Slide</code> to the remote peer.
        /// </summary>
        /// <param name="slide">The slide to send the image's data of.</param>
        private void SendSlideImageData(Slide slide)
        {
            // get the byte[] data
            byte[] slideData = this.GetSlideByteArray(slide);
            // send it
            this.server.SendSlideImageData(slideData);
        }
        /// <summary>
        /// Gets the image data of the entire given <code>Slide</code>.
        /// </summary>
        /// <param name="slide">The <code>Slide</code>.</param>
        /// <returns>The image data of the entire slide.</returns>
        private byte[] GetSlideByteArray(Slide slide)
        {
            // get a temp filename
            String filename = Path.GetTempFileName();

            // export the slide to this file location
            slide.Export(filename, Constants.EXPORT_FILETYPE, Constants.EXPORT_WIDTH, Constants.EXPORT_HEIGHT);

            // read all contents into the buffer
            byte[] buffer = File.ReadAllBytes(filename);
            // and remove the file afterwards, because that's nice
            File.Delete(filename);

            return buffer;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts up the server to be visible in the network.
        /// </summary>
        public void StartServer()
        {
            // get the name of the presentation because it will be announced
            String presentationName = Application.ActivePresentation.Name;
            // actually start the thing
            this.server.Start(presentationName);
        }
        /// <summary>
        /// Stops the server completely.
        /// </summary>
        public void StopServer()
        {
            // "stop"
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
