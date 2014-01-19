using System;
using System.Text;

namespace PowerPoint_Remote
{
    /// <summary>
    /// Constants for runtime.
    /// </summary>
    public abstract class Constants
    {
        /// <summary>
        /// Name of the application.
        /// </summary>
        public const String NAME = "PowerPoint Remote";
        /// <summary>
        /// Defualt encoding used in this application.
        /// </summary>
        public static readonly Encoding ENCODING = Encoding.UTF8;
        public const String CRASH_ERRORTEXT = NAME + " crashed.\n"
            + "This is not the faul of Power Point itself. We're sorry for the inconvenience.\n"
            + "Please restart Power Point and continue working.\n"
            + "\n"
            + "If this crash occured more often plase contact the developer.";

        /// <summary>
        /// The port the server is listening on for clients.
        /// </summary>
        public const int SERVER_PORT = 34012;
        /// <summary>
        /// The length of the pairing code to be generated.
        /// </summary>
        public const int SERVER_PAIRINGCODELENGTH = 6;

        /// <summary>
        /// Filetype of the slide export to be used.
        /// </summary>
        public const String EXPORT_FILETYPE = "PNG";
        /// <summary>
        /// Width of the slide to be exported.
        /// </summary>
        public const int EXPORT_WIDTH = 1024;
        /// <summary>
        /// Height of the slide to be exported.
        /// </summary>
        public const int EXPORT_HEIGHT = 768;

        /// <summary>
        /// The IP range the server is announcing itself on.
        /// 255.255.255.255 is everywhere.
        /// </summary>
        public const String SERVER_IPRANGE = "255.255.255.255";
        /// <summary>
        /// Interval (in ms) the server is announcing itself when no client is connected.
        /// </summary>
        public const int SERVER_ANNOUNCEINTERVAL = 3000;
        /// <summary>
        /// The format string the server should fill when announcing itself.
        /// </summary>
        public const String SERVER_ANNOUNCESTRING = Constants.NAME + "-{0}"; // 0 == presentation name
    }
}
