using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketProxy
{
    /// <summary>
    /// Simplified HttpPacket representation
    /// </summary>
    internal class HttpPacket
    {
        private readonly IDictionary<string, string> _headers;

        public HttpPacket()
        {
            _headers = new Dictionary<string, string>();
        }

        public void AddHeader(string fieldName, string fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(fieldValue))
                return;

            // Http Headers can be repeated per RFC2616
            if (_headers.ContainsKey(fieldName))
            {
                _headers[fieldName] += ',' + fieldValue;
            }
            else
            {
                _headers.Add(fieldName, fieldValue);
            }
        }

        public string GetHeaderOrDefault(string fieldName, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return defaultValue;

            string result;
            return !_headers.TryGetValue(fieldName, out result) ? defaultValue : result;
        }

        public bool IsWebSocketPacket
        {
            get { return _headers.Keys.Any(headerName => WebSocketHeaders.ClientHeaders.Contains(headerName.ToLowerInvariant().Trim())); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in _headers.Keys)
            {
                sb.Append(key);
                sb.Append(" : ");
                sb.Append(_headers[key]);
                sb.Append("\n");
            }

            return string.Format("HttpPacket: \n{0}\n", sb);
        }
    }
}