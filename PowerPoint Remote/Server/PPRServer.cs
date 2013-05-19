using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PowerPoint_Remote.Server
{
    public enum ClientRequest
    {
        StartPresentation,
        StopPresentation,

        NextSlide,
        PreviousSlide,
    }

    public class ClientRequestEventArgs : EventArgs
    {
        public ClientRequest Request
        {
            get;
            set;
        }

        public ClientRequestEventArgs(ClientRequest request)
        {
            this.Request = request;
        }
    }

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

        public delegate void ClientRequestHandler(object sender, ClientRequestEventArgs e);
        public event ClientRequestHandler ClientRequest;
        private void OnClientRequest(ClientRequest request)
        {
            if ( this.ClientRequest != null )
                this.ClientRequest(this, new ClientRequestEventArgs(request));
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
                this.BeginAcceptClient();

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

        private void BeginAcceptClient()
        {
            this.serverSocket.BeginAccept(this.AcceptClient, this.serverSocket);
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
            try
            {
                this.clientSocket.Send(new byte[] { 100 });
            }
            catch ( SocketException )
            {
                // client disconnected
                this.clientSocket = null;
                this.BeginAcceptClient();
                return false;
            }

            int available = this.clientSocket.Available;
            if ( available > 0 )
            {
                byte[] messageIDBuffer = new byte[1];
                this.clientSocket.Receive(messageIDBuffer);
                byte messageID = messageIDBuffer[0];

                switch ( messageID )
                {
                    case 0: // "Init"
                        String pairingCode = this.ReceiveString();

                        Debug.WriteLine(String.Format("Pairing code is '{0}'.", pairingCode));

                        byte accepted = ( this.pairingCode == pairingCode ) ? (byte) 1 : (byte) 0;
                        this.SendMessage(0, accepted);

                        break;

                    case 1: // start
                    case 2: // stop
                    case 3: // next
                    case 4: // prev

                        ClientRequest request = Server.ClientRequest.NextSlide;
                        switch ( messageID )
                        {
                            case 1:
                                request = Server.ClientRequest.StartPresentation;
                                break;
                            case 2:
                                request = Server.ClientRequest.StopPresentation;
                                break;
                            case 3:
                                request = Server.ClientRequest.NextSlide;
                                break;
                            case 4:
                                request = Server.ClientRequest.PreviousSlide;
                                break;
                        }

                        this.OnClientRequest(request);

                        break;

                    default:
                        // unknown
                        break;
                }

                return true;
            }
            else
                return false;
        }

        public void SendSlideNotes(String notes)
        {
            this.SendMessageByte(5);
            this.SendString(notes);
        }

        private void SendMessage(byte messageID)
        {
            this.SendMessageByte(messageID);
        }
        private void SendMessage(byte messageID, byte data)
        {
            this.SendMessageByte(messageID);
            this.SendMessageByte(data);
        }
        private void SendMessage(byte messageID, String data)
        {
            this.SendMessageByte(messageID);

            if ( data != null )
            {
                byte[] dataBuffer = Constants.ENCODING.GetBytes(data);
                this.SendMessageData(dataBuffer);
            }
        }
        private void SendMessageByte(byte b)
        {
            byte[] bBuffer = new byte[1];
            bBuffer[0] = b;
            this.SendMessageData(bBuffer);
        }
        private void SendMessageData(byte[] data)
        {
            this.clientSocket.Send(data);
        }

        private void SendString(String str)
        {
            byte[] strBuffer = Constants.ENCODING.GetBytes(str);
            this.SendMessageByte((byte) strBuffer.Length);
            this.SendMessageData(strBuffer);
        }
        private String ReceiveString()
        {
            byte[] lengthBuffer = new byte[1];
            this.clientSocket.Receive(lengthBuffer);
            int length = lengthBuffer[0];

            if ( length > 0 )
            {
                byte[] dataBuffer = new byte[length];
                this.clientSocket.Receive(dataBuffer);

                return Constants.ENCODING.GetString(dataBuffer);
            }

            return null;
        }
    }
}
