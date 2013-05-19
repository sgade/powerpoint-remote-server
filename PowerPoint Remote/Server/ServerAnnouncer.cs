using System;
using System.Net;
using System.Net.Sockets;

namespace PowerPoint_Remote.Server
{
    public class ServerAnnouncer : IDisposable
    {
        public delegate void BroadcastResponseEventHandler(object sender, EventArgs e);
        public event BroadcastResponseEventHandler BroadcastResponse;
        private void OnBroadcastResponse()
        {
            if ( this.BroadcastResponse != null )
                this.BroadcastResponse(this, EventArgs.Empty);
        }

        private UdpClient client = null;
        private IPEndPoint endPoint = null;
        private byte[] data = null;
        private DateTime lastAnnounce = DateTime.Now;

        public ServerAnnouncer(String ipRange, int port, String presentationName)
        {
            this.Init(ipRange, port, presentationName);
        }
        private void Init(String ipRange, int port, String presentationName)
        {
            this.client = new UdpClient();
            this.endPoint = new IPEndPoint(IPAddress.Parse(ipRange), port);

            String announceString = String.Format(Constants.SERVER_ANNOUNCESTRING, presentationName);
            this.data = Constants.ENCODING.GetBytes(announceString);
        }

        public void Announce()
        {
            TimeSpan elapsedTime = DateTime.Now - this.lastAnnounce;
            if ( elapsedTime.TotalMilliseconds >= Constants.SERVER_ANNOUNCEINTERVAL )
            {
                this.client.Send(this.data, this.data.Length, this.endPoint);

                this.lastAnnounce = DateTime.Now;
            }
        }

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
        ~ServerAnnouncer()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
