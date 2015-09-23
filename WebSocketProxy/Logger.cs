using System;

namespace WebSocketProxy
{
    public class Logger
    {
        public Action<string> Info { get; set; }
        
        public Action<string, Exception> Error { get; set; }

        public Logger()
        {
            Info = (message) => Console.WriteLine("{0}:{1}", DateTime.Now, message);
            Error = (message, error) => Console.WriteLine("{0}:{1} - {2}",
                DateTime.Now, message, error != null ? error.Message : string.Empty);
        }

        public void Log(string message)
        {
            if (Info != null && !string.IsNullOrWhiteSpace(message))
            {
                Info(message);
            }
        }

        public void Warn(string message, Exception e)
        {
            if (Error != null && !string.IsNullOrWhiteSpace(message))
            {
                Error(message, e);
            }
        }
    }
}