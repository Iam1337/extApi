using System;
using System.Net;
using System.Net.Http;
using extApi;
using UnityEngine;

public class Example : MonoBehaviour
{
    private Api _api;

    private void Start()
    {
        _api = new Api();
        _api.AddController(new Test());
        _api.Listen(IPAddress.Loopback, 8080);
        
        Debug.LogFormat("[API] Start listening: http://{0}:{1}/", IPAddress.Loopback, 8080);
    }

    private void OnDestroy()
    {
        _api.Dispose();   
    }
}

[ApiRoute("api")]
public class Test : IApiController
{
    [ApiGet("hello")]
    private ApiResult GetTest()
    {
        return ApiResult.Ok(new Vector3(3,4,5));
    }

    [ApiPost("hello")]
    private ApiResult PostTest()
    {
        return ApiResult.BadRequest();
    }
    
    [ApiPost("hello2")]
    private ApiResult PostTest([ApiBody] RequestClass test)
    {
        Debug.Log($"123 {test.Value}");
        return ApiResult.BadRequest();
    }

    [ApiGet("{name}/potato")]
    private void Get(string name)
    {
        Debug.Log($"NAME: {name}");
    }

    [Serializable]
    public class RequestClass
    {
        public string Value;
    }

    public HttpListenerContext Context { get; set; }
    public HttpListenerRequest Request { get; set; }
    public HttpListenerResponse Response { get; set; }
}