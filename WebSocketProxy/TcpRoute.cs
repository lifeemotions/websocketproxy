using System;

namespace WebSocketProxy
{
    internal class TcpRoute : IDisposable
    {
        private readonly TcpHost _clientMachine;
        private readonly TcpHost _serverMachine;

        #region Events

        public delegate void DisconnectedDelegate(TcpRoute route);

        public event DisconnectedDelegate Disconnected;

        protected void OnDisconnected()
        {
            if (Disconnected != null)
            {
                Disconnected(this);
            }
        }

        #endregion

        public bool Connected
        {
            get { return _clientMachine.Connected && _serverMachine.Connected; }
        }

        public TcpRoute(TcpHost clientMachine, TcpHost serverMachine)
        {
            _clientMachine = clientMachine;
            _serverMachine = serverMachine;
        }

        public void Start()
        {
            RegisterHostEvents();
            _clientMachine.StartReading();
            _serverMachine.StartReading();
        }

        void _serverMachine_DataAvailable(TcpHost host, byte[] data, int length)
        {
            _clientMachine.Send(data, length);
        }

        void _clientMachine_DataAvailable(TcpHost host, byte[] data, int length)
        {
            _serverMachine.Send(data, length);
        }

        void _serverMachine_Disconnected(TcpHost host)
        {
            Stop();
        }

        void _clientMachine_Disconnected(TcpHost host)
        {
            Stop();
        }

        private void RegisterHostEvents()
        {
            _clientMachine.Disconnected += _clientMachine_Disconnected;
            _serverMachine.Disconnected += _serverMachine_Disconnected;

            _clientMachine.DataAvailable += _clientMachine_DataAvailable;
            _serverMachine.DataAvailable += _serverMachine_DataAvailable;
        }
        

        public void Stop()
        {
            _clientMachine.Close();
            _serverMachine.Close();

            OnDisconnected();
        }


        public void Dispose()
        {
            Stop();
        }
    }
}