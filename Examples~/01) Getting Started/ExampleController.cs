using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using extApi;
using UnityEngine;

[ApiRoute("api")]
public class ExampleController
{
    private readonly List<AppModel> _apps = new List<AppModel>
    {
        new() {Id = 0, Name = "App 1", Version = 1},
        new() {Id = 1, Name = "App 2", Version = 6},
        new() {Id = 2, Name = "App 3", Version = 4},
    };

    [ApiGet("start")]
    public ApiResult GetAll()
    {
        Debug.LogFormat("[API] Thread: {0}, Get All Apps", Thread.CurrentThread.ManagedThreadId);
        
        return ApiResult.Ok(new AppsResponse
        {
            Apps = _apps
        });
    }

    [ApiPost("apps")]
    public ApiResult Create([ApiBody] AppCreateRequest request)
    {
        Debug.LogFormat("[API] Thread: {0}, Create App", Thread.CurrentThread.ManagedThreadId);
        
        if (_apps.Any(a => a.Name == request.Name))
            return ApiResult.BadRequest(); // TODO: Already exists
        
        var app = new AppModel
        {
            Id = _apps.Count,
            Name = request.Name,
            Version = request.Version
        };
        
        _apps.Add(app);
        
        return ApiResult.Ok(app);
    }


    [ApiGet("apps/{appId}")]
    public ApiResult GetApp(int appId)
    {
        Debug.LogFormat("[API] Thread: {0}, Get App", Thread.CurrentThread.ManagedThreadId);

        var app = _apps.FirstOrDefault(a => a.Id == appId);
        if (app == null)
            return ApiResult.NotFound(); // TODO: App with id not found
        
        return ApiResult.Ok(app);
    }

    [ApiPost("apps/{appId}")]
    public ApiResult UpdateApp(int appId, [ApiBody] AppUpdateRequest request)
    {
        Debug.LogFormat("[API] Thread: {0}, Update App", Thread.CurrentThread.ManagedThreadId);
        
        var currentApp = _apps.FirstOrDefault(a => a.Name == request.Name);
        var app = _apps.FirstOrDefault(a => a.Id == appId);
        if (app == null)
            return ApiResult.NotFound(); // TODO: App with id not found

        if (currentApp != null && currentApp != app)
            return ApiResult.BadRequest(); // TODO: Name already exists
        
        if (app.Version > request.Version)
            return ApiResult.BadRequest(); // TODO: Low version 

        app.Name = request.Name;
        app.Version = request.Version;
        
        return ApiResult.Ok(app);
    }

    [ApiDelete("apps/{appId}")]
    public ApiResult DeleteApp(int appId)
    {
        Debug.LogFormat("[API] Thread: {0}, Delete App", Thread.CurrentThread.ManagedThreadId);
        
        var app = _apps.FirstOrDefault(a => a.Id == appId);
        if (app == null)
            return ApiResult.NotFound(); // TODO: App with id not found

        _apps.Remove(app);
        return ApiResult.Ok();
    }

    [Serializable]
    public class AppModel
    {
        public int Id;
        public string Name;
        public int Version;
    }

    [Serializable]
    public class AppsResponse
    {
        public List<AppModel> Apps;
    }
    
    [Serializable]
    public class AppCreateRequest
    {
        public string Name;
        public int Version;
    }
    
    [Serializable]
    public class AppUpdateRequest
    {
        public string Name;
        public int Version;
    }
}
