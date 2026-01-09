using System;
using System.Net;
using System.Text;

namespace extApi
{
    public class ApiResult
    {
        // Content
        public static ApiResult Content(HttpStatusCode statusCode, string content) => new(statusCode, new TextPlainContent(content));
        public static ApiResult Content(HttpStatusCode statusCode, string content, Encoding encoding) => new(statusCode, new TextPlainContent(content, encoding));
        
        // 200
        public static ApiResult Ok() => new(HttpStatusCode.OK);
        public static ApiResult Ok(object result) => new(HttpStatusCode.OK, new ApplicationJsonContent(result));
        
        // 201
        // TODO: Add redirection
        public static ApiResult Created() => new(HttpStatusCode.Created);
        public static ApiResult Created(object result) => new(HttpStatusCode.Created, new ApplicationJsonContent(result));
        
        // 202
        // TODO: Add redirection
        public static ApiResult Accepted() => new (HttpStatusCode.Accepted);
        public static ApiResult Accepted(object result) => new(HttpStatusCode.Accepted, new ApplicationJsonContent(result));
        
        // 204
        public static ApiResult NoContent() => new(HttpStatusCode.NoContent);
        public static ApiResult NoContent(object result) => new(HttpStatusCode.NoContent, new ApplicationJsonContent(result));
        
        // 302
        public static ApiResult Redirect(Uri uri) => new(HttpStatusCode.Redirect, uri);
        
        // 400
        public static ApiResult BadRequest() => new(HttpStatusCode.BadRequest);
        public static ApiResult BadRequest(object result) => new(HttpStatusCode.BadRequest, new ApplicationJsonContent(result));

        // 404
        public static ApiResult NotFound() => new(HttpStatusCode.NotFound);
        public static ApiResult NotFound(object result) => new(HttpStatusCode.NotFound, new ApplicationJsonContent(result));
        
        // 500
        public static ApiResult InternalServerError() => new(HttpStatusCode.InternalServerError);

        private readonly HttpStatusCode _statusCode;
        private readonly IResultContent _content;
        private readonly Uri _redirectUri;
        
        private ApiResult(HttpStatusCode statusCode) => _statusCode = statusCode;
        private ApiResult(HttpStatusCode statusCode, IResultContent content) : this(statusCode) => _content = content;
        private ApiResult(HttpStatusCode statusCode, Uri redirectUri) : this(statusCode) => _redirectUri = redirectUri;
        
        internal void Apply(HttpListenerResponse response)
        {
            // Apply status
            response.StatusCode = (int)_statusCode;
            
            // Apply redirection
            if (_redirectUri != null)
            {
                response.RedirectLocation = _redirectUri.ToString();
            }

            // Apply content
            _content?.Apply(response);
        }
    }
}