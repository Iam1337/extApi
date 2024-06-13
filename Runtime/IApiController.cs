using System.Net;

namespace extApi
{
    public interface IApiController
    {
        HttpListenerContext Context { get; set; }
        HttpListenerRequest Request { get; set; }
        HttpListenerResponse Response { get; set; }
    }
}