using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PowerPoint_Remote.Server
{
    public class PPRServer
    {
        #region Events
        public delegate void StartedEventHandler(object sender, StartedEventArgs e);
        public event StartedEventHandler Started;
        private void OnStarted()
        {
            if ( this.Started != null )
                this.Started(this, new StartedEventArgs(this.pairingCode));
        }
        public delegate void StoppedEventHandler(object sender, EventArgs e);
        public event StoppedEventHandler Stopped;
        private void OnStopped()
        {
            if ( this.Stopped != null )
                this.Stopped(this, EventArgs.Empty);
        }
        #endregion

        private Thread thread = null;
        private Socket serverSocket = null;
        private ServerAnnouncer announcer = null;
        private String pairingCode = null;
        private Socket clientSocket = null;

        public PPRServer()
        {
        }

        public bool isRunning()
        {
            return ( this.thread != null && this.thread.IsAlive );
        }

        public void Start(String presentationName)
        {
            if ( !this.isRunning() )
            {
                this.announcer = new ServerAnnouncer(Constants.SERVER_IPRANGE, Constants.SERVER_PORT, presentationName);

                this.thread = new Thread(this.Run);
                this.thread.Name = "PPRServer";
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
            this.pairingCode = PairingCodeGenerator.GenerateCode();
            this.OnStarted();

            try
            {
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.serverSocket.Bind(new IPEndPoint(IPAddress.Any, Constants.SERVER_PORT));
                this.serverSocket.Listen(1);
                this.serverSocket.BeginAccept(this.AcceptClient, this.serverSocket);

                while ( true )
                {
                    if ( !this.AnnounceOrHandle() )
                        Thread.Sleep(500);
                }
            }
            catch ( ThreadInterruptedException )
            {
                // abort, it's okay
                this.serverSocket.Close();
            }
            finally
            {
                this.OnStopped();
            }
        }

        private void AcceptClient(IAsyncResult ar)
        {
            if ( ar.IsCompleted && ar.AsyncState != null )
            {
                try
                {
                    Socket serverSocket = (Socket) ar.AsyncState;
                    this.clientSocket = serverSocket.EndAccept(ar);
                }
                catch ( ObjectDisposedException )
                {
                    // ignore, accept was aborted
                }
            }
        }

        private bool AnnounceOrHandle()
        {
            if ( this.clientSocket == null )
            {
                this.announcer.Announce();

                return false;
            }
            else
                return this.HandleClient();
        }
        private bool HandleClient()
        {
            int available = this.serverSocket.Available;
            if ( available > 0 )
            {
                byte[] data = new byte[available];

                this.serverSocket.Receive(data);

                Debug.WriteLine("Recieved: " + Constants.ENCODING.GetString(data));

                return true;
            }
            else
                return false;
        }
    }
}
