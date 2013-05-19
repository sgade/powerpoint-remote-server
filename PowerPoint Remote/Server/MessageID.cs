namespace PowerPoint_Remote.Server
{
    // Min = 0, Max = 254 (BYTE)
    public enum MessageID
    {
        Init = 0,

        Start,
        Stop,
        Next,
        Prev,

        Notes,
        Image,

        Ping = 100,
    }
}
