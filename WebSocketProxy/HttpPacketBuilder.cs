using System;

namespace WebSocketProxy
{
    internal class HttpPacketBuilder
    {
        public static HttpPacket BuildPacket(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return null;

            try
            {
                HttpPacket result = new HttpPacket();

                string[] splitted = data.Split('\n');
                
                // There are no headers... nothing to do here...
                if (splitted.Length < 2)
                    return null;

                for (int i = 1; i < splitted.Length; i++)
                {
                    string header = splitted[i];
                    
                    // End of HTTP headers (newline)
                    if (string.IsNullOrWhiteSpace(header))
                        break;

                    string[] splittedHeader = header.Split(':');

                    if (splittedHeader.Length < 2)
                        continue;

                    // header name
                    string fieldName = splittedHeader[0].Trim().ToLowerInvariant();

                    // header value
                    string value = string.Empty;
                    for (int j = 1; j < splittedHeader.Length; j++)
                    {
                        value = value + splittedHeader[j];
                    }

                    result.AddHeader(fieldName, value.Trim());
                }
                
                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}