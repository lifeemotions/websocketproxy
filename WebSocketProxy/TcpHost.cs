using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace WebSocketProxy
{
    internal class TcpHost : IDisposable
    {
        private bool _closed;
        
        #region Events

        public delegate void DataAvailableDelegate(TcpHost host, byte[] data, int length);

        public event DataAvailableDelegate DataAvailable;

        protected void OnDataAvailable(byte[] data, int length)
        {
            if (DataAvailable != null)
            {
                DataAvailable(this, data, length);
            }
        }

        public delegate void DisconnectedDelegate(TcpHost host);

        public event DisconnectedDelegate Disconnected;

        protected void OnDisconnected()
        {
            if (Disconnected != null)
            {
                Disconnected(this);
            }
        }

        #endregion

        private const int DefaultBufferLength = 4 * 1024;

        private readonly TcpClient _tcpClient;
        private readonly byte[] _buffer;

        public Stream Stream { get; private set; }

        public bool Connected
        {
            get { return _tcpClient.Connected; }
        }

        #region Constructors

        public static TcpHost ManufactureDefault(IPAddress address, int port)
        {
            return new TcpHost(new TcpClient(address.ToString(), port));
        }

        public TcpHost(TcpClient tcpClient)
        {
            _buffer = new byte[DefaultBufferLength];
            _tcpClient = tcpClient;
            Stream = _tcpClient.GetStream();
        }
        

        #endregion


        public async Task AuthenticateAsync(X509Certificate2 certificate)
        {
            if (certificate == null)
                return;

            SslStream sslStream = new SslStream(Stream, false);
            Stream = sslStream;
            await sslStream.AuthenticateAsServerAsync(certificate);   
        }

        public void BeginAuthenticationAsServer(X509Certificate2 certificate, SslProtocols protocols, AsyncCallback callback, object state)
        {
            SslStream sslStream = new SslStream(Stream, false);
            Stream = sslStream;
            sslStream.BeginAuthenticateAsServer(certificate, false, protocols, false, callback, state);
        }

        public void EndAuthenticationAsServer(IAsyncResult asyncResult)
        {
            SslStream sslStream = Stream as SslStream;
            if (sslStream != null)
            {
                sslStream.EndAuthenticateAsServer(asyncResult);
            }

            Stream = sslStream;
        }

        public void Send(byte[] data, int length)
        {
            try
            {
                if (!_closed)
                {
                    Stream.Write(data, 0, length);
                }
                
            }
            catch (Exception)
            {
                OnDisconnected();
            }
            
        }

        public void StartReading()
        {
            try
            {
                if (!_closed)
                {
                    Stream.BeginRead(_buffer, 0, _buffer.Length, ReadAsyncCallback, Stream);
                }
            }
            catch (Exception)
            {
                OnDisconnected();
            }
            
        }

        private void ReadAsyncCallback(IAsyncResult asyncResult)
        {
            if (_tcpClient.Connected && !_closed)
            {
                try
                {
                    int numberOfBytesRead = Stream.EndRead(asyncResult);

                    // No more data to read. Close the connection
                    if (numberOfBytesRead > 0)
                    {
                        OnDataAvailable(_buffer, numberOfBytesRead);
                        StartReading();
                    }
                    else
                    {
                        Close();
                        OnDisconnected();
                    }


                }
                catch (IOException)
                {
                    OnDisconnected();
                }
                catch (ObjectDisposedException)
                {
                    // 
                }
            }
            else
            {
                OnDisconnected();
            }
        }

        public void Close()
        {
            try
            {
                if (_tcpClient == null || !_tcpClient.Connected) return;

                _closed = true;
                _tcpClient.Close();
            }
            catch (Exception)
            {
                //
            }
            
        }

        public void Dispose()
        {
            Close();
        }
    }
}