using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPoint_Remote.Server
{
    public class StartedEventArgs : EventArgs
    {
        public String PairingCode
        {
            get;
            set;
        }

        public StartedEventArgs(String pairingCode)
        {
            this.PairingCode = pairingCode;
        }
    }
}
