
using System;

namespace extApi
{
    public static class ApiUtils
    {
        public static string Combine(string uri1, string uri2) => $"{uri1.TrimEnd('/')}/{uri2.TrimStart('/')}";

        public static object CreateDefault(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}