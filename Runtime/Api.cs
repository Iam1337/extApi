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
        private readonly ThreadMode _threadMode;
        
        private Thread _listenerThread;
        private readonly object _listenerThreadLock = new();
        private readonly Queue<ApiSession> _listenerThreadQueue = new();
        
        private Thread _responseThread;
        private readonly object _responseThreadLock = new();
        private readonly Queue<ApiSession> _responseThreadQueue = new();
        
        private CancellationTokenSource _cancellationSource;
        private CancellationToken _cancellationToken;

        private readonly ApiRouteNode _root = new("/");


        public Api() : this(ThreadMode.OtherThread) { }
        public Api(ThreadMode mode) => _threadMode = mode;

        public void Listen(ushort port, params IPAddress[] addresses)
        {
            if (_listener != null)
                throw new Exception("Already started");

            _listener = new HttpListener();
            foreach (var address in addresses)
                _listener.Prefixes.Add($"http://{address}:{port}/");
            _listener.Start();

            _cancellationSource = new CancellationTokenSource();
            _cancellationToken = _cancellationSource.Token;
            
            _listenerThread = new Thread(ListenProcess) { Name = "extApi Listen Thread" };
            _listenerThread.Start();

            _responseThread = new Thread(ResponseProcess) { Name = "extApi Response Thread" };
            _responseThread.Start();
        }

        public void Update()
        {
            if (_threadMode != ThreadMode.MainThread)
                throw new Exception($"Available only in {nameof(ThreadMode.MainThread)} mode");

            while (true)
            {
                var session = (ApiSession) null;
                lock (_listenerThreadLock)
                {
                    if (!_listenerThreadQueue.TryDequeue(out session))
                        break;
                }

                ProcessSession(session);
            }
        }

        public void Close()
        {
            _listener?.Stop();
            _listener = null;
            _cancellationSource.Cancel();
            _cancellationSource = null;
            _listenerThread.Abort();
            _listenerThread = null;
            _responseThread.Abort();
            _responseThread = null;
        }

        public void AddController(object controller)
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
                        var routeNode = CreateRouteNode(routePath);
                        if (routeNode == null)
                            throw new Exception("Route build failed");

                        if (routeNode.Methods.ContainsKey(methodAttribute.Method))
                            throw new Exception($"Path \"{routePath}\" already has \"{methodAttribute.Method}\" method");

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
            while (!_cancellationToken.IsCancellationRequested)
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
                            var session = target.GetSession(context, routeParameters);

                            if (_threadMode == ThreadMode.OtherThread)
                            {
                                ProcessSession(session); // TODO: Сделать асинхронный отлов. 
                            }
                            else
                            {
                                lock (_listenerThreadLock)
                                {
                                    _listenerThreadQueue.Enqueue(session);
                                }
                            }
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
                { }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
        
        private void ResponseProcess()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ApiSession session;
                    
                    lock (_responseThreadLock)
                    {
                        if (!_responseThreadQueue.TryDequeue(out session))
                            continue;
                    }

                    session.Context.Response.ContentType = "application/json";
                    session.Context.Response.StatusCode = (int)session.Result.StatusCode;

                    if (session.Result.Json != null)
                    {
                        var json = session.Result.Json;
                        var jsonData = Encoding.UTF8.GetBytes(json);

                        session.Context.Response.ContentLength64 = jsonData.Length;
                        session.Context.Response.OutputStream.Write(jsonData);
                        session.Context.Response.OutputStream.Flush();
                    }

                    session.Context.Response.Close();
                }
                catch (ThreadAbortException)
                { }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        private void ProcessSession(ApiSession session)
        {
            try
            {
                var resultBoxed = session.MethodInfo.Invoke(session.Controller, session.Arguments);
                if (resultBoxed != null)
                {
                    var resultType = resultBoxed.GetType();
                    if (resultType == typeof(ApiResult) ||
                        resultType.IsSubclassOf(typeof(ApiResult)))
                    {
                        session.Result = (ApiResult)resultBoxed;
                    }
                    else
                    {
                        session.Result = ApiResult.Ok(resultBoxed);
                    }
                }
                else
                {
                    session.Result = ApiResult.Ok();
                }
            }
            catch
            {
                session.Result = ApiResult.InternalServerError();
            }

            lock (_responseThreadLock)
            {
                _responseThreadQueue.Enqueue(session);
            }
        }

        private ApiRouteNode CreateRouteNode(string route)
        {
            return string.IsNullOrEmpty(route) ? _root : GetRouteNode(route.Split('/'), true, null);
        }

        private ApiRouteTarget GetRouteTarget(HttpMethod method, IReadOnlyList<string> segments, IDictionary<string, string> parameters)
        {
            var node = GetRouteNode(segments, false, parameters);
            return node?.Methods.GetValueOrDefault(method);
        }

        private ApiRouteNode GetRouteNode(IReadOnlyList<string> segments, bool create, IDictionary<string, string> parameters)
        {
            if (segments[0] == "/" && segments.Count == 1)
                return _root;

            var startIndex = segments[0] == "/" ? 1 : 0;
            var currentNode = _root;

            for (var i = startIndex; i < segments.Count; i++)
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

        public void Dispose() => Close();
    }
}