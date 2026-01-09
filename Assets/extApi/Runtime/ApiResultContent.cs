using System.Net;
using System.Text;
using UnityEngine;

namespace extApi
{
    internal interface IResultContent
    {
        void Apply(HttpListenerResponse response);
    }
    
    // "application/json"
    internal class ApplicationJsonContent : IResultContent
    {
        private readonly object _content;
        private readonly Encoding _encoding;
        
        public ApplicationJsonContent(object content) : this(content, Encoding.UTF8) { }
        
        public ApplicationJsonContent(object content, Encoding encoding)
        {
            _content = content;
            _encoding = encoding;
        }

        public void Apply(HttpListenerResponse response)
        {
            var json = JsonUtility.ToJson(_content, false); // TODO: IJsonSerializer in Api
            var data = _encoding.GetBytes(json);

            response.ContentType = ApiContentType.Application.Json;
            response.ContentLength64 = data.Length;
            response.OutputStream.Write(data, 0, data.Length);
            response.OutputStream.Flush();
        }
    }
    
    // "text/plain"
    internal class TextPlainContent : IResultContent
    {
        private readonly string _content;
        private readonly Encoding _encoding;

        public TextPlainContent(string content) : this(content, Encoding.UTF8){ }
        
        public TextPlainContent(string content, Encoding encoding)
        {
            _content = content;
            _encoding = encoding;
        }
        
        public void Apply(HttpListenerResponse response)
        {
            var data = _encoding.GetBytes(_content);
            
            response.ContentType = ApiContentType.Text.Plain;
            response.ContentLength64 = data.Length;
            response.OutputStream.Write(data, 0, data.Length);
            response.OutputStream.Flush();
        }
    }
}