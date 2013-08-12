using System;
using System.Text;

namespace PowerPoint_Remote
{
    public abstract class Constants
    {
        public const String NAME = "PowerPoint Remote";
        public static readonly Encoding ENCODING = Encoding.UTF8;

        public const int SERVER_PORT = 34012;
        public const int SERVER_PAIRINGCODELENGTH = 6;

        public const String EXPORT_FILETYPE = "PNG";
        public const int EXPORT_WIDTH = 1024;
        public const int EXPORT_HEIGHT = 768;

        public const String SERVER_IPRANGE = "255.255.255.255";
        public const int SERVER_ANNOUNCEINTERVAL = 3000;
        public const String SERVER_ANNOUNCESTRING = Constants.NAME + "-{0}"; // 0 == presentation name
    }
}
