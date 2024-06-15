using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace extApi
{
    internal class ApiRouteNode
    {
        private ApiCors _cors;
        private readonly ApiRouteNode _parent;
        
        public readonly string Name;
        public readonly bool IsDynamic;
        public readonly string DynamicName;
        public readonly List<ApiRouteNode> Nodes = new();
        public readonly Dictionary<HttpMethod, ApiRouteTarget> Methods = new();

        public ApiRouteNode(string name, ApiRouteNode parent)
        {
            Name = name;
            IsDynamic = name.StartsWith('{') && name.EndsWith('}');
            DynamicName = name.Trim('{', '}');
            
            _parent = parent;
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