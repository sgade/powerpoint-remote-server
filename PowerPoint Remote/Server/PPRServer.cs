using System;
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

    public class PPRServer : IDisposable
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

        #region Main Loop
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
                byte messageIDByte = this.ReceiveByte();
                MessageID messageID = (MessageID) messageIDByte;

                switch ( messageID )
                {
                    case MessageID.Init:
                        String pairingCode = this.ReceiveString();

                        byte accepted = ( this.pairingCode == pairingCode ) ? (byte) 1 : (byte) 0;
                        this.SendMessage(MessageID.Init, accepted);

                        break;

                    case MessageID.Start:
                    case MessageID.Stop:
                    case MessageID.Next:
                    case MessageID.Prev:

                        ClientRequest request = Server.ClientRequest.NextSlide;
                        switch ( messageID )
                        {
                            case MessageID.Start:
                                request = Server.ClientRequest.StartPresentation;
                                break;
                            case MessageID.Stop:
                                request = Server.ClientRequest.StopPresentation;
                                break;
                            case MessageID.Next:
                                request = Server.ClientRequest.NextSlide;
                                break;
                            case MessageID.Prev:
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
        #endregion

        #region Public Send Methods
        public void SendSlideNotes(String notes)
        {
            this.SendMessage(MessageID.Notes);
            this.SendString(notes);
        }
        public void SendSlideImageData(byte[] data)
        {
            this.SendMessage(MessageID.Image);
            this.SendMessageData(data);
        }
        #endregion

        #region Send
        #region HighLevel
        private void SendMessage(MessageID messageID)
        {
            this.SendMessageByte((byte) messageID);
        }
        private void SendMessage(MessageID messageID, byte data)
        {
            this.SendMessage(messageID);
            this.SendMessageByte(data);
        }
        private void SendMessage(MessageID messageID, String data)
        {
            this.SendMessage(messageID);

            byte[] dataBuffer = Constants.ENCODING.GetBytes(data);
            this.SendMessageData(dataBuffer);
        }
        #endregion

        #region LowLevel
        private void SendString(String str)
        {
            byte[] strBuffer = Constants.ENCODING.GetBytes(str);
            this.SendMessageByte((byte) strBuffer.Length);
            this.SendMessageData(strBuffer);
        }

        private void SendMessageByte(byte b)
        {
            byte[] bBuffer = new byte[] { b };
            this.SendMessageData(bBuffer);
        }

        private void SendMessageData(byte[] data)
        {
            this.clientSocket.Send(data);
        }
        #endregion
        #endregion

        #region Receive
        private byte ReceiveByte()
        {
            byte[] buffer = new byte[1];
            this.clientSocket.Receive(buffer);

            return buffer[0];
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
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if ( disposing )
            {
                if ( this.clientSocket != null )
                    this.clientSocket.Close();

                if ( this.serverSocket != null )
                    this.serverSocket.Close();

                this.announcer.Dispose();
            }
        }
        ~PPRServer()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
