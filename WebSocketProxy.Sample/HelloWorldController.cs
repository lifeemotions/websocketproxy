using Nancy;

namespace WebSocketProxy.Sample
{
    public class HelloWorldController : NancyModule
    {
        public HelloWorldController()
        {
            Get["/"] = v => View["Content/index.html"];
        }
    }
}