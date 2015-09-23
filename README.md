WebSocketProxy ![build status](https://travis-ci.org/lifeemotions/websocketproxy.svg?branch=master) 
=======

WebSocketProxy is a lightweight C# library which allows you to connect to an HTTP server and websocket server through the same TCP port.
This library is independent on which web frameworks are used for handling websocket and http requests.

# Installation

Installation can be done using NuGet.

```
Install-Package WebSocketProxy
```

# Sample Usage
The project "WebSocket.Sample" contains a sample usage using [NancyFX](https://github.com/NancyFx/Nancy) and [Fleck](https://github.com/statianzo/Fleck).

The first step is to build the configuration object in which you set the endpoints that are listening for HTTP (Nancy) and WebSocket requests (Fleck) and the public endpoint which will be listening to both kinds of requests. 
```csharp
TcpProxyConfiguration configuration = new TcpProxyConfiguration()
            {
                PublicHost = new Host()
                {
                    IpAddress = IPAddress.Parse("0.0.0.0"),
                    Port = 8080
                },
                HttpHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = 8081
                },
                WebSocketHost = new Host()
                {

                    IpAddress = IPAddress.Loopback,
                    Port = 8082
                }
            };

```
Then, initialize Nancy and Fleck, followed by the WebSocketServer.

```csharp
            using (var nancyHost = new NancyHost(new Uri("http://localhost:8081")))
            using (var websocketServer = new WebSocketServer("ws://0.0.0.0:8082"))
            using (var tcpProxy = new TcpProxyServer(configuration))
            {
                // Initialize Nancy
                nancyHost.Start();
                
                // Initialize Fleck
                websocketServer.Start(connection =>
                {
                    connection.OnOpen = () => Console.WriteLine("Connection on open");
                    connection.OnClose = () => Console.WriteLine("Connection on close");
                    connection.OnMessage = message => Console.WriteLine("Message: " + message);
                });

                // Initialize the proxy
                tcpProxy.Start();

                Console.WriteLine("Press [Enter] to stop");
                Console.ReadLine();
            }
```
By pointing the web browser to the port 8080, you will receive an html page which initializes a websocket connection.

# SSL Support
This library supports HTTPS and WSS. Simply pass the certificate path and password in the configuration object and all incoming requests will be unencrypted and routed to the corresponding library.

