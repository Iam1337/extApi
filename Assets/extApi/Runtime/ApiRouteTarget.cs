using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using UnityEngine;

namespace extApi
{
    internal class ApiRouteTarget
    {
        public object Controller;
        public MethodInfo MethodInfo;
        public ParameterInfo[] ParameterInfos;

        public ApiSession GetSession(HttpListenerContext context, Dictionary<string, string> routeParameters)
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
                if (parameterInfo.GetCustomAttribute<ApiBodyAttribute>() == null)
                {
                    args.Add(routeParameters.TryGetValue(parameterInfo.Name, out var value)
                        ? TypeDescriptor.GetConverter(parameterType).ConvertFromString(null, CultureInfo.InvariantCulture, value)
                        : ApiUtils.CreateDefault(parameterInfo.ParameterType));
                }
                else
                {
                    if (context.Request.HasEntityBody)
                    {
                        using var stream = context.Request.InputStream;
                        using var reader = new StreamReader(stream, context.Request.ContentEncoding);

                        var contentType = context.Request.ContentType;
                        if (contentType != null)
                        {
                            try
                            {
                                // TODO: Add new contentTypes
                                if (contentType.StartsWith(ApiContentType.Application.Json, StringComparison.OrdinalIgnoreCase))
                                {
                                    args.Add(JsonUtility.FromJson(reader.ReadToEnd(), parameterType)); // TODO: IJsonSerializer in Api
                                }
                                else
                                {
                                    Debug.LogWarning("Unsupported content type: " + contentType);
                                    args.Add(ApiUtils.CreateDefault(parameterType));
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Unable to parse content type: " + contentType + "\n" + e);
                                args.Add(ApiUtils.CreateDefault(parameterType));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Content type not found");
                            args.Add(ApiUtils.CreateDefault(parameterType));
                        }
                    }
                    else
                    {
                        args.Add(ApiUtils.CreateDefault(parameterType));
                    }
                }
            }

            return new ApiSession
            {
                Controller = Controller,
                MethodInfo = MethodInfo,
                Context = context,
                Arguments = args.ToArray()
            };
        }
    }
}