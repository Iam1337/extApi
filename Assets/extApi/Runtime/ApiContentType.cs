namespace extApi
{
    public static class ApiContentType
    {
        public static class Application
        {
            public const string Json = "application/json";
        }

        public static class Text
        {
            public const string Plain = "text/plain";
            public const string Html = "text/html"; // TODO: ...
        }
        
        public static class Multipart
        {
            public const string FormData = "multipart/form-data";
        }
    }
}