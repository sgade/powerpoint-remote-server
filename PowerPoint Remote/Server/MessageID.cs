namespace PowerPoint_Remote.Server
{
    // Min = 0, Max = 254 (BYTE)
    /// <summary>
    /// The Messages, that the server will understand.
    /// This enum makes it easier to understand the bytes that come and go from and to the client.
    /// </summary>
    public enum MessageID
    {
        // Handshake
        Init = 0,

        // Commands to control PowerPoint.
        // the first two are also sent to the client to indicate status
        Start,
        Stop,
        Next,
        Prev,

        // data of the slide
        Notes,
        Image,

        // to notice whether the client is still there
        Ping = 100,
    }
}
