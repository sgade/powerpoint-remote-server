using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PowerPoint_Remote.Server
{
    /// <summary>
    /// Defines the types of requests the client could make to the server.
    /// </summary>
    public enum ClientRequest
    {
        StartPresentation,
        StopPresentation,

        NextSlide,
        PreviousSlide,
    }

    /// <summary>
    /// Event arguments for when the server was booted up.
    /// </summary>
    public class StartedEventArgs : EventArgs
    {
        /// <summary>
        /// The generated pairing code.
        /// </summary>
        public String PairingCode
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pairingCode">The paring code generated during load.</param>
        public StartedEventArgs(String pairingCode)
        {
            this.PairingCode = pairingCode;
        }
    }
    public class ClientStatusEventArgs : EventArgs
    {
        public bool ClientConnected
        {
            get;
            set;
        }

        public ClientStatusEventArgs(bool clientConnected)
        {
            this.ClientConnected = clientConnected;
        }
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

    /// <summary>
    /// Main server class.
    /// Handles everything related to the remote peer.
    /// </summary>
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

        public delegate void ClientStatusEventHandler(object sender, ClientStatusEventArgs e);
        public event ClientStatusEventHandler ClientStatus;
        private void OnClientStatusChanged(bool clientConnected)
        {
            if ( this.ClientStatus != null )
            {
                this.ClientStatus(this, new ClientStatusEventArgs(clientConnected));
            }
        }

        public delegate void ClientRequestHandler(object sender, ClientRequestEventArgs e);
        public event ClientRequestHandler ClientRequest;
        private void OnClientRequest(ClientRequest request)
        {
            if ( this.ClientRequest != null )
                this.ClientRequest(this, new ClientRequestEventArgs(request));
        }
        #endregion

        /// <summary>
        /// Reference to the worker thread that is not killing other application's activity.
        /// </summary>
        private Thread thread = null;
        /// <summary>
        /// The server socket that can accept other connections.
        /// </summary>
        private Socket serverSocket = null;
        /// <summary>
        /// The <code>ServerAnnouncer</code> used to highlight this instance of PPR.
        /// </summary>
        private ServerAnnouncer announcer = null;
        /// <summary>
        /// The pairing code generated on startup.
        /// </summary>
        private String pairingCode = null;
        /// <summary>
        /// The ONE and ONLY client connection (because we only have one at a time) <code>Socket</code>.
        /// </summary>
        private Socket clientSocket = null;
        /// <summary>
        /// Indicates whether the client has already been accepted by us, the server.
        /// </summary>
        private bool clientAccepted = false;

        /// <summary>
        /// Returns whether the server is currently running.
        /// </summary>
        /// <returns>Whether the server is currently running.</returns>
        public bool isRunning()
        {
            // thread was created and is still running
            return ( this.thread != null && this.thread.IsAlive );
        }

        /// <summary>
        /// Start up the server with all its components.
        /// </summary>
        /// <param name="presentationName">The name of the current presentation.</param>
        public void Start(String presentationName)
        {
            // if we are not running already
            if ( !this.isRunning() )
            {
                if ( this.announcer != null )
                    this.announcer.Dispose(); // clear resources
                // create the server announcer based on our presentation name
                this.announcer = new ServerAnnouncer(Constants.SERVER_IPRANGE, Constants.SERVER_PORT, presentationName);

                // create and start new thread
                this.thread = new Thread(this.Run);
                this.thread.Name = "PPRServer";
                this.thread.Start();
            }
        }
        /// <summary>
        /// Stops the server and all of its components.
        /// </summary>
        public void Stop()
        {
            // if we are in fact running
            if ( this.isRunning() )
            {
                // interrupt the thread
                this.thread.Interrupt();
                // wait for it but at max 1s
                this.thread.Join(1000);
            }
        }

        #region Main Loop
        /// <summary>
        /// Main working routine
        /// </summary>
        private void Run()
        {
            // generate a new pairing code
            this.pairingCode = PairingCodeGenerator.GenerateCode();
            // send a new notification to listeners
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
                if ( this.clientSocket != null )
                {
                    this.clientSocket.Close();
                    this.clientSocket = null;
                }
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

                    this.OnClientStatusChanged(true);
                }
                catch ( ObjectDisposedException )
                {
                    // ignore, accept was aborted
                }
            }
        }
        
        private void OnClientDisconnected()
        {
            this.clientSocket = null;
            this.clientAccepted = false;

            this.OnClientStatusChanged(false);
            this.BeginAcceptClient();
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
            if ( this.clientAccepted )
            {
                try
                {
                    this.SendMessage(MessageID.Ping);
                }
                catch ( SocketException )
                {
                    // client disconnected
                    this.OnClientDisconnected();
                    return false;
                }
            }

            int available = this.clientSocket.Available;
            if ( available > 0 )
            {
                byte messageIDByte = this.ReceiveByte();
                MessageID messageID = (MessageID) messageIDByte;

                if ( this.clientAccepted )
                {
                    switch ( messageID )
                    {
                        case MessageID.Init:
                            // not relevant
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
                }
                else
                {
                    if ( messageID == MessageID.Init )
                    {
                        String pairingCode = this.ReceiveString();

                        this.clientAccepted = ( this.pairingCode == pairingCode );
                        byte accepted = ( this.clientAccepted ) ? (byte) 1 : (byte) 0;
                        this.SendMessage(MessageID.Init, accepted);
                    }
                }

                return true;
            }
            else
                return false;
        }
        #endregion

        #region Public Send Methods
        public void SendStop()
        {
            this.SendMessage(MessageID.Stop);
        }
        public void SendSlideNotes(String notes)
        {
            this.SendMessage(MessageID.Notes);
            this.SendString(notes);
        }
        public void SendSlideImageData(byte[] data)
        {
            this.SendMessage(MessageID.Image);
            this.SendMessageInt(data.Length);
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
            this.SendMessageInt(strBuffer.Length);
            this.SendMessageData(strBuffer);
        }

        private byte[] EncodeInt(int value)
        {
            byte[] intBuffer = new byte[32];

            for ( int i = 0; i < intBuffer.Length; i++ )
            {
                int FLAG = ( 1 << i );
                bool isSet = ( value & FLAG ) == FLAG;
                intBuffer[i] = (byte) ( isSet ? 1 : 0 );
            }

            return intBuffer;
        }
        public void SendMessageInt(int value)
        {
            byte[] encodedData = this.EncodeInt(value);
            this.SendMessageData(encodedData);
        }
        private void SendMessageByte(byte b)
        {
            byte[] bBuffer = new byte[] { b };
            this.SendMessageData(bBuffer);
        }

        private void SendMessageData(byte[] data)
        {
            if ( this.clientSocket != null )
            {
                int tries = 0;
                int sent = 0;

                while ( sent < data.Length )
                {
                    sent += this.clientSocket.Send(data, data.Length - sent, SocketFlags.None);

                    tries++;
                }

                if ( tries > 0 )
                {
                    Console.WriteLine("Wrote to socket " + tries + " times to send " + sent + " bytes.");
                }
            }
        }
        #endregion
        #endregion

        #region Receive
        private String ReceiveString()
        {
            int length = this.ReceiveInt();

            if ( length > 0 )
            {
                byte[] dataBuffer = new byte[length];
                this.clientSocket.Receive(dataBuffer);

                return Constants.ENCODING.GetString(dataBuffer);
            }

            return null;
        }

        private int DecodeInt(byte[] intBuffer)
        {
            int value = 0;

            for ( int i = 0; i < intBuffer.Length; i++ )
            {
                if ( intBuffer[i] == 1 )
                {
                    value += (int) ( Math.Pow(2, i) );
                }
            }

            return value;
        }
        private int ReceiveInt()
        {
            byte[] intBuffer = new byte[32];

            for ( int i = 0; i < intBuffer.Length; i++ )
            {
                intBuffer[i] = this.ReceiveByte();
            }

            return this.DecodeInt(intBuffer);
        }
        private byte ReceiveByte()
        {
            byte[] buffer = this.ReceiveByteData(1);

            if ( buffer.Length >= 1 )
                return buffer[0];
            else
                throw new ArgumentNullException("No data received.");
        }

        private byte[] ReceiveByteData(int length)
        {
            try
            {
                byte[] buffer = new byte[length];
                this.clientSocket.Receive(buffer, length, SocketFlags.None);

                return buffer;
            }
            catch ( SocketException )
            {
                // error
            }

            return new byte[0];
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
