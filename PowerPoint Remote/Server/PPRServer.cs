using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PowerPoint_Remote.Server
{
    public class PPRServer
    {
        public delegate void StartedEventHandler(object sender, EventArgs e);
        public event StartedEventHandler Started;
        private void OnStarted()
        {
            if ( this.Started != null )
                this.Started(this, EventArgs.Empty);
        }
        public delegate void StoppedEventHandler(object sender, EventArgs e);
        public event StoppedEventHandler Stopped;
        private void OnStopped()
        {
            if ( this.Stopped != null )
                this.Stopped(this, EventArgs.Empty);
        }

        private Thread thread = null;
        private Socket socket = null;

        public bool isRunning()
        {
            return ( this.thread.IsAlive );
        }

        public PPRServer()
        {
            this.thread = new Thread(this.Run);
            this.thread.Name = "PPRServer";
        }

        public void Start()
        {
            if ( !this.isRunning() )
            {
                this.thread.Start();
            }
        }
        public void Stop()
        {
            if ( this.isRunning() )
            {
                this.thread.Interrupt();
                this.thread.Join(1000);
            }
        }

        private void Run()
        {
            this.OnStarted();

            try
            {
                Thread.Sleep(50000);
            }
            //catch ( ThreadInterruptedException )
            catch ( TypeAccessException )
            {
                // abort, it's okay
            }
            finally
            {
                this.OnStopped();
            }
        }
    }
}
