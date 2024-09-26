using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace extApi
{
    public class ApiSession
    {
        // shared
        public HttpListenerContext Context;
        
        // input
        public object Controller;
        public MethodInfo MethodInfo;
        public object[] Arguments;

        // output
        public ApiResult Result;
    }
}