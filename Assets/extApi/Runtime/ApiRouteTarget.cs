﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using UnityEngine;

namespace extApi
{
    public class ApiRouteTarget
{
    public object Controller;
    public MethodInfo MethodInfo;
    public ParameterInfo[] ParameterInfos;

    public ApiResult Invoke(HttpListenerContext context, Dictionary<string, string> routeParameters)
    {
        var args = new List<object>();
        
        if (Controller is IApiController apiController)
        {
            apiController.Context = context;
            apiController.Request = context.Request;
            apiController.Response = context.Response;
        }
        
        foreach (var parameterInfo in ParameterInfos)
        {
            var parameterType = parameterInfo.ParameterType;
            if (parameterType.GetCustomAttribute<ApiBodyAttribute>() == null)
            {
                args.Add(routeParameters.TryGetValue(parameterInfo.Name, out var value)
                    ? TypeDescriptor.GetConverter(parameterType).ConvertFromString(value)
                    : ApiUtils.CreateDefault(parameterInfo.ParameterType));
            }
            else
            {
                if (context.Request.HasEntityBody)
                {
                    using var stream = context.Request.InputStream;
                    using var reader = new StreamReader(stream, context.Request.ContentEncoding);

                    try
                    {
                        args.Add(JsonUtility.FromJson(reader.ReadToEnd(), parameterType));
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("Unable to parse"); // TODO: More info
                        args.Add(ApiUtils.CreateDefault(parameterType));
                    }
                }
                else
                {
                    args.Add(ApiUtils.CreateDefault(parameterType));
                }
            }
        }
        
        var resultBoxed = MethodInfo.Invoke(Controller, args.ToArray());
        if (resultBoxed != null)
        {
            var resultType = resultBoxed.GetType();
            if (resultType == typeof(ApiResult) || 
                resultType.IsSubclassOf(typeof(ApiResult)))
            {
                return (ApiResult)resultBoxed;
            }

            return ApiResult.Ok(resultBoxed);
        }
        
        return ApiResult.Ok();
    }
}
}