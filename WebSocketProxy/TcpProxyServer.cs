using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WebSocketProxy
{
    public class TcpProxyServer : IDisposable
    {
        private readonly TcpListener _tcpListener;
        private readonly TcpProxyConfiguration _configuration;
        private X509Certificate2 _certificate;
        private readonly TcpConnectionManager _tcpConnectionManager;
        private bool _closing;

        private readonly Logger _logger;

        public Logger Log
        {
            get { return _logger; }
        }


        public int ConnectionCount
        {
            get { return _tcpConnectionManager.ConnectionCount; }
        }

        #region Constructors

        public TcpProxyServer(TcpProxyConfiguration configuration)
        {
            _logger = new Logger();
            _tcpConnectionManager = new TcpConnectionManager();
            _configuration = configuration;
            _tcpListener = new TcpListener(_configuration.PublicHost.IpAddress, _configuration.PublicHost.Port);
        }

        #endregion

        public void Start()
        {
            if (_configuration.EnableSslCertificate)
            {
                _certificate = _configuration.SslCertificatePassword != null
                    ? new X509Certificate2(_configuration.SslCertificatePath, _configuration.SslCertificatePassword)
                    : new X509Certificate2(_configuration.SslCertificatePath);
            }

            _tcpListener.Start();
            DoBeginListenForClients();

            Log.Info(string.Format("Proxy Server started at {0}", _configuration.PublicHost));
        }

        private void DoBeginListenForClients()
        {
            try
            {
                _tcpListener.BeginAcceptTcpClient(TcpClientAcceptCallback, _tcpListener);
            }
            catch (InvalidOperationException)
            {
                // socket has been closed
            }
            catch (IOException)
            {
                //
            }
        }

        private void TcpClientAcceptCallback(IAsyncResult asyncResult)
        {
            if (_closing)
                return;

            DoBeginListenForClients();

            try
            {
                TcpClient client = _tcpListener.EndAcceptTcpClient(asyncResult);

                TcpHost host = new TcpHost(client);
                if (_certificate != null)
                {
                    host.BeginAuthenticationAsServer(_certificate, _configuration.SslProtocols, AuthenticationCallback,
                        host);
                }
                else
                {
                    ReadFirstPacket(host);
                }
            }
            catch (IOException)
            {
                // TCP Listener may not be valid anymore
            }
        }

        private void AuthenticationCallback(IAsyncResult asyncResult)
        {
            if (asyncResult == null) return;

            TcpHost host = asyncResult.AsyncState as TcpHost;
            if (host == null)
                return;

            try
            {
                host.EndAuthenticationAsServer(asyncResult);
                ReadFirstPacket(host);
            }
            catch (IOException)
            {
                // Somehow, the authentication failed... Close the connection
                host.Close();
            }
        }

        private void ReadFirstPacket(TcpHost clientMachine)
        {
            byte[] data = new byte[4 * 1024];

            try
            {
                TcpStateContainer state = new TcpStateContainer(clientMachine, data);
                clientMachine.Stream.BeginRead(data, 0, data.Length, FirstPacketReadCallback, state);
            }
            catch (IOException)
            {
                clientMachine.Close();
            }
        }

        private void FirstPacketReadCallback(IAsyncResult asyncResult)
        {
            if (asyncResult == null) return;

            TcpStateContainer asyncState = asyncResult.AsyncState as TcpStateContainer;
            if (asyncState == null)
                return;

            TcpHost clientMachine = asyncState.Client;
            byte[] data = asyncState.DataBuffer;

            try
            {
                int numBytes = clientMachine.Stream.EndRead(asyncResult);
                string stringData = Encoding.UTF8.GetString(data, 0, numBytes);

                HttpPacket packet = HttpPacketBuilder.BuildPacket(stringData);

                if (packet == null)
                {
                    clientMachine.Close();
                    return;
                }

                Host serverMachineHost = packet.IsWebSocketPacket
                    ? _configuration.WebSocketHost
                    : _configuration.HttpHost;

                if (serverMachineHost != null && serverMachineHost.IsSpecified)
                {
                    TcpHost serverMachine = TcpHost.ManufactureDefault(serverMachineHost.IpAddress,
                        serverMachineHost.Port);

                    serverMachine.Send(data, numBytes);

                    TcpRoute route = new TcpRoute(clientMachine, serverMachine);
                    _tcpConnectionManager.AddRoute(route);
                }
                else
                {
                    clientMachine.Close();
                }
            }
            catch (IOException) {}
        }

        public void Dispose()
        {
            if (_tcpListener == null)
                return;

            _closing = true;

            try
            {
                _tcpListener.Stop();
            }
            catch (IOException)
            {
                // In case the listener is not listening anymore
            }
        }
    }
}
