using System.Collections.Generic;

namespace WebSocketProxy
{
    internal class TcpConnectionManager
    {
        private readonly ICollection<TcpRoute> _connections;
        
        public TcpConnectionManager()
        {
            _connections = new List<TcpRoute>();
        }

        public int ConnectionCount
        {
            get
            {
                lock(_connections)
                {
                    return _connections.Count;
                }
            }
        }

        public void AddRoute(TcpRoute route)
        {
            if (route == null || !route.Connected) return;
            
            lock (_connections)
            {
                route.Disconnected += route_Disconnected;
                _connections.Add(route);

                route.Start();
            }
        }

        void route_Disconnected(TcpRoute route)
        {
            if (route == null) return;
            
            lock (_connections)
            {
                route.Disconnected -= route_Disconnected;
                _connections.Remove(route);
            }
        }
        
    }
}