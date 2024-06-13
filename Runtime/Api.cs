using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace extApi
{
    public class Api : IDisposable
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private CancellationTokenSource _listenerCancellationSource;
        private CancellationToken _listenerCancellationToken;

        private readonly ApiRouteNode _root = new("/");

        public void Listen(IPAddress address, ushort port)
        {
            if (_listener != null)
                throw new Exception("Already started");

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{address}:{port}/");
            _listener.Start();

            _listenerCancellationSource = new CancellationTokenSource();
            _listenerCancellationToken = _listenerCancellationSource.Token;
            _listenerThread = new Thread(ListenProcess);
            _listenerThread.Start();
        }

        public void AddController(IApiController controller)
        {
            var controllerType = controller.GetType();
            var controllerRouteAttributes = controllerType.GetCustomAttributes<ApiRouteAttribute>().ToList();
            if (controllerRouteAttributes.Any() == false)
                throw new NullReferenceException(nameof(ApiRouteAttribute)); // No attribute

            var controllerMethods = controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

            foreach (var routeAttribute in controllerRouteAttributes)
            {
                foreach (var controllerMethod in controllerMethods)
                {
                    var methodAttributes = controllerMethod.GetCustomAttributes<ApiMethodAttribute>(true).ToList();
                    if (methodAttributes.Any() == false)
                        continue;

                    foreach (var methodAttribute in methodAttributes)
                    {
                        var routePath = ApiUtils.Combine(routeAttribute.Route, methodAttribute.Template);
                        var routeNode = GetRouteNode(routePath, true);
                        if (routeNode == null)
                            throw new Exception($"Route build failed");

                        if (routeNode.Methods.ContainsKey(methodAttribute.Method))
                            throw new Exception(
                                $"Path \"{routePath}\" already has \"{methodAttribute.Method}\" method");

                        routeNode.Methods.Add(methodAttribute.Method, new ApiRouteTarget
                        {
                            Controller = controller,
                            MethodInfo = controllerMethod,
                            ParameterInfos = controllerMethod.GetParameters(),
                        });
                    }
                }
            }
        }

        private void ListenProcess()
        {
            while (!_listenerCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = _listener.GetContext();
                    var contextMethod = new HttpMethod(context.Request.HttpMethod);

                    var routeUri = context.Request.Url;
                    var routeParameters = new Dictionary<string, string>();
                    var target = GetRouteTarget(contextMethod, routeUri.Segments, routeParameters);
                    if (target != null)
                    {
                        try
                        {
                            var result = target.Invoke(context, routeParameters);

                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = (int)result.StatusCode;

                            if (result.Result != null)
                            {
                                var json = JsonUtility.ToJson(result.Result, false);
                                var jsonData = Encoding.UTF8.GetBytes(json);

                                context.Response.ContentLength64 = jsonData.Length;
                                context.Response.OutputStream.Write(jsonData);
                                context.Response.OutputStream.Flush();
                            }

                            context.Response.Close();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);

                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.Close();
                        }
                    }
                    else
                    {
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.Response.Close();
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        private ApiRouteNode GetRouteNode(string route, bool create)
        {
            return string.IsNullOrEmpty(route) ? _root : GetRouteNode(route.Split('/'), create, null);
        }

        private ApiRouteNode GetRouteNode(string[] segments, bool create, Dictionary<string, string> parameters)
        {
            if (segments[0] == "/" && segments.Length == 1)
                return _root;

            var startIndex = segments[0] == "/" ? 1 : 0;
            var currentNode = _root;

            for (var i = startIndex; i < segments.Length; i++)
            {
                var name = segments[i].TrimEnd('/');
                var node = currentNode.Nodes.Find(n => n.Name == name);
                if (node == null)
                {
                    if (create)
                    {
                        node = new ApiRouteNode(name);
                        node.IsDynamic = name.StartsWith('{') && name.EndsWith('}');
                        node.DynamicName = name.Trim('{', '}');

                        currentNode.Nodes.Add(node);
                    }
                    else
                    {
                        if (parameters != null)
                        {
                            node = currentNode.Nodes.Find(n => n.IsDynamic);
                            if (node != null)
                            {
                                parameters.Add(node.DynamicName, name);
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                currentNode = node;
            }

            return currentNode;
        }

        private ApiRouteTarget GetRouteTarget(HttpMethod method, string[] segments, Dictionary<string, string> parameters)
        {
            var node = GetRouteNode(segments, false, parameters);
            return node?.Methods.GetValueOrDefault(method);
        }
        

        public void Dispose()
        {
            _listener?.Stop();
            _listenerCancellationSource.Cancel();
            _listenerThread.Abort();
        }
    }
}