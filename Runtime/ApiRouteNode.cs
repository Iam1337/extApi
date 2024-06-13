using System.Collections.Generic;
using System.Net.Http;

namespace extApi
{
    public class ApiRouteNode
    {
        public readonly string Name;
        public readonly List<ApiRouteNode> Nodes = new();
        public readonly Dictionary<HttpMethod, ApiRouteTarget> Methods = new();

        public bool IsDynamic;
        public string DynamicName;

        public ApiRouteNode(string name) => Name = name;
    }
}