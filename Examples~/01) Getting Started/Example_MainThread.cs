using extApi;
using UnityEngine;

public class Example_MainThread : MonoBehaviour
{
    private Api _api;

    private void Start()
    {
        _api = new Api(ThreadMode.MainThread);
        _api.AddController(new ExampleController());
        _api.Listen(8080);
    }

    private void Update()
    {
        _api.Update();
    }

    private void OnDestroy()
    {
        _api.Close();   
    }
}