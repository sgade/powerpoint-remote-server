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
                // create the listening server socket
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // bind it to the address
                this.serverSocket.Bind(new IPEndPoint(IPAddress.Any, Constants.SERVER_PORT));
                this.serverSocket.Listen(1);
                // begin async accept routine
                this.BeginAcceptClient();

                // FOREVER!
                while ( true )
                {
                    // do some work
                    if ( !this.AnnounceOrHandle() )
                    {
                        // or else, if 'false' is returned,
                        // just chill a moment because obviously
                        // there's nothing to do and we really 
                        // don't want to waste precious CPU
                        // time for something like PowerPoint
                        // right?
                        Thread.Sleep(500);
                    }
                }
            }
            catch ( ThreadInterruptedException )
            {
                // aborted, it's okay
                // but close the sockets properly
                this.serverSocket.Close();
                if ( this.clientSocket != null )
                {
                    this.clientSocket.Close();
                    this.clientSocket = null;
                }
            }
            finally
            {
                // always send notification
                this.OnStopped();
            }
        }

        /// <summary>
        /// Starts the async operation of accepting the client socket by the server socket.
        /// </summary>
        private void BeginAcceptClient()
        {
            this.serverSocket.BeginAccept(this.AcceptClient, this.serverSocket);
        }
        /// <summary>
        /// Callback for when the async Accept of the client was successful.
        /// </summary>
        /// <param name="ar">The async result.</param>
        private void AcceptClient(IAsyncResult ar)
        {
            // if the connect is completed
            if ( ar.IsCompleted && ar.AsyncState != null )
            {
                try
                {
                    // get the server socket
                    Socket serverSocket = (Socket) ar.AsyncState;
                    // and finally make it accept the client (socket)
                    this.clientSocket = serverSocket.EndAccept(ar);

                    // notify that we now are connected
                    this.OnClientStatusChanged(true);
                }
                catch ( ObjectDisposedException )
                {
                    // ignore, accept was aborted
                }
            }
        }
        
        /// <summary>
        /// Clears up all cariables and takes appropriate steps when the client has disconnected.
        /// </summary>
        private void OnClientDisconnected()
        {
            // set values
            this.clientSocket = null;
            this.clientAccepted = false;

            // send event
            this.OnClientStatusChanged(false);
            // accept new client
            this.BeginAcceptClient();
        }

        /// <summary>
        /// Decides which work is to be done.
        /// </summary>
        /// <returns>Whether some time consuming operation was executed or not.</returns>
        private bool AnnounceOrHandle()
        {
            // if we do not have a client connected
            if ( this.clientSocket == null )
            {
                // announce ourself
                this.announcer.Announce();

                // but we really did not operate anything
                return false;
            }
            else
            {
                // because we have a connection, let's handle any data in or out
                return this.HandleClient();
            }
        }
        /// <summary>
        /// Manages everything related to the client <code>Socket</code>.
        /// </summary>
        /// <returns>Whether some time consuming operation was executed or not.</returns>
        private bool HandleClient()
        {
            // if we have already accepted the client
            if ( this.clientAccepted )
            {
                try
                {
                    // ping it, to check connection
                    this.SendMessage(MessageID.Ping);
                }
                catch ( SocketException )
                {
                    // exception, so: client disconnected
                    // cleanup
                    this.OnClientDisconnected();
                    return false; // we didn't do anything
                }
            }

            // get the available bytes to read
            int available = this.clientSocket.Available;
            // if we have something waiting for us...
            if ( available > 0 )
            {
                // receive the first (message) byte
                byte messageIDByte = this.ReceiveByte();
                // convert it corresponding to the ENUM
                MessageID messageID = (MessageID) messageIDByte;

                if ( this.clientAccepted )
                {
                    // only if we already have accepted the client,
                    // these messages are ok
                    switch ( messageID )
                    {
                        case MessageID.Init:
                            // not relevant
                            break;

                        // any state-changing command...
                        case MessageID.Start:
                        case MessageID.Stop:
                        case MessageID.Next:
                        case MessageID.Prev:

                            // .. is to be converted into ClientRequest enum...
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

                            // ...and sent to the listeners
                            this.OnClientRequest(request);

                            break;

                        default:
                            // unknown
                            break;
                    }
                }
                else
                {
                    // because the client is not accepted,
                    // only Init is to be expected
                    if ( messageID == MessageID.Init )
                    {
                        // get the pairing code from the remote
                        String pairingCode = this.ReceiveString();

                        // check the validity of the code
                        this.clientAccepted = ( this.pairingCode == pairingCode );
                        // send the init answer to the client
                        byte accepted = ( this.clientAccepted ) ? (byte) 1 : (byte) 0;
                        this.SendMessage(MessageID.Init, accepted);
                    }
                }

                // we probably did some sending or somethin', so we 'true' out
                return true;
            }
            else
                return false; // no data, no computation
        }
        #endregion

        #region Public Send Methods
        /// <summary>
        /// Sends the 'Stop' command to the client.
        /// </summary>
        public void SendStop()
        {
            this.SendMessage(MessageID.Stop);
        }
        /// <summary>
        /// Sends the notes message to the client.
        /// </summary>
        /// <param name="notes">The notes to be sent.</param>
        public void SendSlideNotes(String notes)
        {
            this.SendMessage(MessageID.Notes);
            this.SendString(notes);
        }
        /// <summary>
        /// Sends the image message to the client.
        /// </summary>
        /// <param name="data">The corresponding image data to be sent.</param>
        public void SendSlideImageData(byte[] data)
        {
            this.SendMessage(MessageID.Image);
            this.SendMessageInt(data.Length);
            this.SendMessageData(data);
        }
        #endregion
        #region Send
        #region HighLevel
        /// <summary>
        /// Sends the specified <code>MessageID</code> to the client.
        /// </summary>
        /// <param name="messageID">The <code>MessageID</code> to be sent.</param>
        private void SendMessage(MessageID messageID)
        {
            // convert it to byte
            this.SendMessageByte((byte) messageID);
        }
        /// <summary>
        /// Sends the specified <code>MessageID</code> with some additional <code>byte</code> data to the client.
        /// </summary>
        /// <param name="messageID">The <code>MessageID</code> to be sent.</param>
        /// <param name="data">The data to be sent with the message.</param>
        private void SendMessage(MessageID messageID, byte data)
        {
            this.SendMessage(messageID);
            this.SendMessageByte(data);
        }
        #endregion

        #region LowLevel
        /// <summary>
        /// Sends the specified <code>String</code> according to the protocol to the client.
        /// </summary>
        /// <param name="str">The <code>String</code> to be sent.</param>
        private void SendString(String str)
        {
            // convert to bytes, according to encoding
            byte[] strBuffer = Constants.ENCODING.GetBytes(str);
            // send int length first
            this.SendMessageInt(strBuffer.Length);
            // then the raw data itself
            this.SendMessageData(strBuffer);
        }

        /// <summary>
        /// Encodes the given <code>int</code> value according to the protocol into a <code>byte</code> array.
        /// </summary>
        /// <param name="value">The <code>int</code> value to be encoded.</param>
        /// <returns>The <code>byte</code> array representing "<code>value</code>".</returns>
        private byte[] EncodeInt(int value)
        {
            // bytes as bit (probably too much)
            byte[] intBuffer = new byte[32];

            // get each bit
            for ( int i = 0; i < intBuffer.Length; i++ )
            {
                // is the bit set?
                int FLAG = ( 1 << i );
                bool isSet = ( value & FLAG ) == FLAG;

                // save into buffer
                intBuffer[i] = (byte) ( isSet ? 1 : 0 );
            }

            return intBuffer;
        }
        /// <summary>
        /// Sends an <code>Integer</code> value to the client.
        /// </summary>
        /// <param name="value">The <code>int</code> to be sent.</param>
        public void SendMessageInt(int value)
        {
            // encode the int
            byte[] encodedData = this.EncodeInt(value);
            // send it as bytes
            this.SendMessageData(encodedData);
        }
        /// <summary>
        /// Sends a single <code>byte</code> to the client.
        /// </summary>
        /// <param name="b">The <code>byte</code> to be sent.</param>
        private void SendMessageByte(byte b)
        {
            // wrap the byte in an array
            byte[] bBuffer = new byte[] { b };
            this.SendMessageData(bBuffer);
        }

        /// <summary>
        /// Sends <code>byte</code>s to the client.
        /// </summary>
        /// <param name="data">Raw bytes to be sent.</param>
        private void SendMessageData(byte[] data)
        {
            // only if a client connection is existant
            if ( this.clientSocket != null )
            {
                // bytes already sent
                int sent = 0;

                // loop for all bytes
                while ( sent < data.Length )
                {
                    // send max number of bytes
                    sent += this.clientSocket.Send(data, data.Length - sent, SocketFlags.None);
                }
            }
        }
        #endregion
        #endregion

        #region Receive
        /// <summary>
        /// Receives a <code>String</code> from the client.
        /// </summary>
        /// <returns>The successfully received <code>String</code>.</returns>
        private String ReceiveString()
        {
            // get the buffer length first
            int length = this.ReceiveInt();

            // if we have some valid data (hopefully)
            if ( length > 0 )
            {
                // create the buffer
                byte[] dataBuffer = new byte[length];
                // and read from the stream
                this.clientSocket.Receive(dataBuffer);

                // finally, decode the string back from bytes
                return Constants.ENCODING.GetString(dataBuffer);
            }

            // something went wrong
            return null;
        }

        /// <summary>
        /// Decodes the given <code>byte</code> array according to the protocol into a <code>int</code> value.
        /// </summary>
        /// <param name="intBuffer">The <code>byte</code> array representing the <code>int</code> value.</param>
        /// <returns>The <code>int</code> value that was decoded.</returns>
        private int DecodeInt(byte[] intBuffer)
        {
            // the value at the end
            int value = 0;

            // check each register
            for ( int i = 0; i < intBuffer.Length; i++ )
            {
                // if FLAG is true
                if ( intBuffer[i] == 1 )
                {
                    // calculate bin to int
                    value += (int) ( Math.Pow(2, i) );
                }
            }

            return value;
        }
        /// <summary>
        /// Receives an <code>int</code> from the client.
        /// </summary>
        /// <returns>The received <code>int</code>.</returns>
        private int ReceiveInt()
        {
            // receive 32 bytes
            byte[] intBuffer = new byte[32];

            for ( int i = 0; i < intBuffer.Length; i++ )
            {
                intBuffer[i] = this.ReceiveByte();
            }

            // decode the buffer
            return this.DecodeInt(intBuffer);
        }
        /// <summary>
        /// Receives a <code>bytes</code> from the client.
        /// </summary>
        /// <returns>The <code>bytes</code> received.</returns>
        private byte ReceiveByte()
        {
            // read a buffer of 1
            byte[] buffer = this.ReceiveByteData(1);

            // if we have something
            if ( buffer.Length >= 1 )
                return buffer[0]; // return it
            else
                throw new ArgumentNullException("No data received."); // oh no!
        }

        /// <summary>
        /// Receives a <code>byte</code> array from the client.
        /// </summary>
        /// <param name="length">The length of data to be received</param>
        /// <returns></returns>
        private byte[] ReceiveByteData(int length)
        {
            try
            {
                // create buffer
                byte[] buffer = new byte[length];
                // read buffer
                this.clientSocket.Receive(buffer, length, SocketFlags.None);

                return buffer;
            }
            catch ( SocketException )
            {
                // error
            }

            // 0 bytes, error but still valid
            return new byte[0];
        }
        #endregion

        // this interface has to be implemented since we are dealing with Socket connections...
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
