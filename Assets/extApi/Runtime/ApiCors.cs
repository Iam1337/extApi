using System.Net;
using System.Net.Http;

namespace extApi
{
    internal class ApiCors
    {
        private const string Separator = ", ";
        private const string AllowOrigin = "Access-Control-Allow-Origin";
        private const string AllowMethods = "Access-Control-Allow-Methods";
        private const string AllowHeaders = "Access-Control-Allow-Headers";
        private const string MaxAge = "Access-Control-Allow-Max-Age";
        
        private readonly string[] _origin;
        private readonly string[] _headers;
        private readonly string[] _methods;
        private readonly int _maxAge;
        public ApiCors(string[] origin, string[] headers, string[] methods, int maxAge = 172800)
        {
            _origin = origin;
            _headers = headers;
            _methods = methods;
            _maxAge = maxAge;
        }
        
        public void ApplyCors(HttpMethod method, HttpListenerContext context)
        {
            var headers = context.Response.Headers;
            if (method == HttpMethod.Options)
            {
                if (_headers != null && CanWrite(AllowHeaders))
                    headers.Add(AllowHeaders, string.Join(Separator, _headers));

                if (_methods != null && CanWrite(AllowMethods))
                    headers.Add(AllowMethods, string.Join(Separator, _methods));

                if (CanWrite(MaxAge))
                    headers.Add(MaxAge, _maxAge.ToString());
            }
            
            if (_origin != null && CanWrite(AllowOrigin))
                headers.Add(AllowOrigin, string.Join(Separator, _origin));

            bool CanWrite(string header) => !header.Contains(header);
        }
    }
}