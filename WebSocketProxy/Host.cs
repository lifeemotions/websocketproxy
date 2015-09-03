using System.Net;

namespace WebSocketProxy
{
    public class Host
    {
        public Host()
        {
            Port = -1;
        }
        
        public IPAddress IpAddress { get; set; }

        public int Port { get; set; }

        public bool IsSpecified
        {
            get
            {
                return Port != -1 && IpAddress != null;
            } 
        } 

        public override string ToString()
        {
            return IpAddress + ":" + Port;
        }
    }
}