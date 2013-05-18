using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PowerPoint_Remote.Server
{
    public class ServerAnnouncer
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
    }
}
