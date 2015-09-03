namespace WebSocketProxy
{
    /// <summary>
    /// Helper class for managing the state in a Stream.BeginRead operation
    /// </summary>
    internal class TcpStateContainer
    {
        public TcpStateContainer(TcpHost client, byte[] dataBuffer)
        {
            Client = client;
            DataBuffer = dataBuffer;
        }

        public TcpHost Client { get; set; }

        public byte[] DataBuffer { get; set; }
    }
}