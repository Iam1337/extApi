using System.Net;
using extApi;
using UnityEngine;

public class Example_Simple : MonoBehaviour
{
    private Api _api;

    private void Start()
    {
        _api = new Api();
        _api.AddController(new ExampleController());
        _api.Listen(8080, IPAddress.Any, IPAddress.Loopback);
    }

    private void OnDestroy()
    {
        _api.Close();   
    }
}