using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PowerPoint_Remote.Server
{
    public class PPRServer
    {
        private Thread thread = null;
        private Socket socket = null;

        public bool isRunning()
        {
            return ( this.thread != null && this.thread.IsAlive );
        }

        public PPRServer()
        {
            
        }

        public bool Start()
        {
            if ( !this.isRunning() )
            {
                this.thread = new Thread(this.Run);
                this.thread.Start();

                return true;
            }

            return false;
        }
        public bool Stop()
        {
            if ( this.isRunning() )
            {
                this.thread.Interrupt();
                this.thread.Join(1000);

                return true;
            }

            return false;
        }

        private void Run()
        {
            Debug.WriteLine("running!");
        }
    }
}
