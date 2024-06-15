using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using UnityEngine;

namespace extApi
{
    internal class ApiRouteTarget
    {
        private ApiCors _cors;
        
        private readonly ApiRouteNode _parent;
        private readonly object _controller;
        private readonly MethodInfo _methodInfo;
        private readonly ParameterInfo[] _parameterInfos;


        public ApiRouteTarget(ApiRouteNode parent, object controller, MethodInfo methodInfo)
        {
            _parent = parent;
            _controller = controller;
            _methodInfo = methodInfo;
            _parameterInfos = methodInfo.GetParameters();
        }

        public ApiResult Invoke(HttpListenerContext context, Dictionary<string, string> routeParameters)
        {
            var args = new List<object>();

            if (_controller is IApiController apiController)
            {
                apiController.Context = context;
                apiController.Request = context.Request;
                apiController.Response = context.Response;
            }

            foreach (var parameterInfo in _parameterInfos)
            {
                var parameterType = parameterInfo.ParameterType;
                if (parameterInfo.GetCustomAttribute<ApiBodyAttribute>() == null)
                {
                    args.Add(routeParameters.TryGetValue(parameterInfo.Name, out var value)
                        ? TypeDescriptor.GetConverter(parameterType)
                            .ConvertFromString(null, CultureInfo.InvariantCulture, value)
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

            var resultBoxed = _methodInfo.Invoke(_controller, args.ToArray());
            if (resultBoxed != null)
            {
                var resultType = resultBoxed.GetType();
                if (resultType == typeof(ApiResult) ||
                    resultType.IsSubclassOf(typeof(ApiResult)))
                {
                    return (ApiResult) resultBoxed;
                }

                return ApiResult.Ok(resultBoxed);
            }

            return ApiResult.Ok();
        }

        public void SetCors(ApiCors cors) => _cors = cors;

        public bool HasCors() => _cors != null || (_parent?.HasCors() ?? false);

        public void ApplyCors(HttpMethod method, HttpListenerContext context)
        {
            _cors?.ApplyCors(method, context);
            _parent?.ApplyCors(method, context);
        }
    }
}