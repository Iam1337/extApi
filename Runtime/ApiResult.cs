using System.Net;

namespace extApi
{
    public class ApiResult
    {
        public static ApiResult Ok() => new(HttpStatusCode.OK);
        public static ApiResult Ok(object result) => new(HttpStatusCode.OK, result);
        public static ApiResult BadRequest() => new(HttpStatusCode.BadRequest);
        public static ApiResult NotFound() => new(HttpStatusCode.NotFound);

        public HttpStatusCode StatusCode { get; }
        public object Result { get; }

        private ApiResult(HttpStatusCode statusCode) => StatusCode = statusCode;
        private ApiResult(HttpStatusCode statusCode, object result) : this(statusCode) => Result = result;

        public override string ToString() => $"<{StatusCode}: {Result}>";
    }
}