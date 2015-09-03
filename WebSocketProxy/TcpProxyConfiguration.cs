using System.Security.Authentication;

namespace WebSocketProxy
{
    public class TcpProxyConfiguration
    {
        private const SslProtocols DefaultProtocols = SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

        #region Constructors

        public TcpProxyConfiguration() : this(DefaultProtocols)
        {
            //
        }

        public TcpProxyConfiguration(SslProtocols protocols)
        {
            SslProtocols = protocols;
        }

        #endregion


        /// <summary>
        /// Public interface in which the proxy will be listening
        /// </summary>
        public Host PublicHost { get; set; }

        /// <summary>
        /// Host which, when specified, will redirect the WebSocket packets
        /// </summary>
        public Host WebSocketHost { get; set; }

        /// <summary>
        /// Host which, when specified, will redirect plain HTTP packets
        /// </summary>
        public Host HttpHost { get; set; }

        /// <summary>
        /// If true, all packets will be decrypted using the SSL certificate specified in <see cref="SslCertificatePath"/>
        /// </summary>
        public bool EnableSslCertificate
        {
            get { return SslCertificatePath != null; }  
        }

        /// <summary>
        /// FileSystem path to the SSL certificate
        /// </summary>
        public string SslCertificatePath { get; set; }

        /// <summary>
        /// Instalation Password of the certificate specified in <see cref="SslCertificatePath"/>
        /// </summary>
        public string SslCertificatePassword { get; set; }

        /// <summary>
        /// Enabled SSL protocols, in case EnableSslCertificate is set to true
        /// </summary>
        public SslProtocols SslProtocols { get; set; }
    }
}