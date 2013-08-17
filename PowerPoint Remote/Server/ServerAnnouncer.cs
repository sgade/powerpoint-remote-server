using System;
using System.Net;
using System.Net.Sockets;

namespace PowerPoint_Remote.Server
{
    /// <summary>
    /// The class taking care of UDP broadcasting for the server.
    /// </summary>
    public class ServerAnnouncer : IDisposable
    {
        /// <summary>
        /// The <code>UdpClient</code> sending the packets.
        /// </summary>
        private UdpClient client = null;
        /// <summary>
        /// The <code>IPEndPoint</code> to send the packets to.
        /// </summary>
        private IPEndPoint endPoint = null;
        /// <summary>
        /// The <code>byte[]</code> data to be sent with each broadcast.
        /// </summary>
        private byte[] data = null;
        /// <summary>
        /// The time of the last announcement packet send.
        /// </summary>
        private DateTime lastAnnounce = DateTime.Now;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ipRange">The IP address range the broadcast packages should be sent to.</param>
        /// <param name="port">The port the packages should be sent on.</param>
        /// <param name="presentationName">The name of the presentation which will be announced to listeners.</param>
        public ServerAnnouncer(String ipRange, int port, String presentationName)
        {
            this.Init(ipRange, port, presentationName);
        }
        /// <summary>
        /// Initializes the class' attributes for further use.
        /// </summary>
        /// <param name="ipRange">The IP range to broadcast to.</param>
        /// <param name="port">The port to broadcast on.</param>
        /// <param name="presentationName">The name of the current presentation.</param>
        private void Init(String ipRange, int port, String presentationName)
        {
            // create the client
            this.client = new UdpClient();
            // and the endpoint to the ip range
            this.endPoint = new IPEndPoint(IPAddress.Parse(ipRange), port);

            // the string that will be sent each time
            String announceString = String.Format(Constants.SERVER_ANNOUNCESTRING, presentationName);
            // converted into bytes...
            this.data = Constants.ENCODING.GetBytes(announceString);
        }

        /// <summary>
        /// Announces the computer on the network according to the preferences of the class.
        /// </summary>
        public void Announce()
        {
            // time since the last announcement, we do not want to flood the network
            TimeSpan elapsedTime = DateTime.Now - this.lastAnnounce;
            if ( elapsedTime.TotalMilliseconds >= Constants.SERVER_ANNOUNCEINTERVAL )
            {
                // send the broadcast data
                this.client.Send(this.data, this.data.Length, this.endPoint);

                // and save current time
                this.lastAnnounce = DateTime.Now;
            }
        }

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
                if ( this.client != null )
                    this.client.Close();
            }
        }
        /// <summary>
        /// Deconstructor.
        /// </summary>
        ~ServerAnnouncer()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
