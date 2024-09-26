using System.Net;
using UnityEngine;

namespace extApi
{
    public class ApiResult
    {
        public static ApiResult Ok() => new(HttpStatusCode.OK);
        public static ApiResult Ok(object result) => new(HttpStatusCode.OK, result);
        public static ApiResult BadRequest() => new(HttpStatusCode.BadRequest);
        public static ApiResult NotFound() => new(HttpStatusCode.NotFound);
        public static ApiResult InternalServerError() => new(HttpStatusCode.InternalServerError);

        public HttpStatusCode StatusCode { get; }
        public string Json { get; }

        private ApiResult(HttpStatusCode statusCode) => StatusCode = statusCode;
        private ApiResult(HttpStatusCode statusCode, object result) : this(statusCode) => Json = JsonUtility.ToJson(result, false);
    }
}