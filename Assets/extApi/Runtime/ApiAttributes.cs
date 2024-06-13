using System;
using System.Net.Http;

namespace extApi
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ApiBodyAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ApiRouteAttribute : Attribute
    {
        public ApiRouteAttribute(string route) => Route = route;
        public readonly string Route;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ApiMethodAttribute : Attribute
    {
        public readonly HttpMethod Method;
        public readonly string Template;

        protected ApiMethodAttribute(HttpMethod method, string template)
        {
            Method = method;
            Template = template;
        }
    }

    public class ApiHeadAttribute : ApiMethodAttribute
    {
        public ApiHeadAttribute(string template) : base(HttpMethod.Head, template)
        { }
    }

    public class ApiGetAttribute : ApiMethodAttribute
    {
        public ApiGetAttribute(string template) : base(HttpMethod.Get, template)
        { }
    }

    public class ApiPostAttribute : ApiMethodAttribute
    {
        public ApiPostAttribute(string template) : base(HttpMethod.Post, template)
        { }
    }

    public class ApiPutAttribute : ApiMethodAttribute
    {
        public ApiPutAttribute(string template) : base(HttpMethod.Put, template)
        { }
    }

    public class ApiDeleteAttribute : ApiMethodAttribute
    {
        public ApiDeleteAttribute(string template) : base(HttpMethod.Delete, template)
        { }
    }
}