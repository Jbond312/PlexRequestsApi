using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace PlexRequests.Api
{
    public class ApiRequest
    {
        public ApiRequest(string endpoint, string baseUri, HttpMethod httpMethod, Dictionary<string, string> requestHeaders, Dictionary<string, string> contentHeaders, object body)
        {
            Endpoint = endpoint;
            BaseUri = baseUri;
            HttpMethod = httpMethod;
            RequestHeaders = requestHeaders;
            ContentHeaders = contentHeaders;
            Body = body;
        }

        public string Endpoint { get; }
        public string BaseUri { get; }
        public HttpMethod HttpMethod { get; }
        public Dictionary<string, string> RequestHeaders { get; }
        public Dictionary<string, string> ContentHeaders { get; }
        public object Body { get; }

        public string FullUri
        {
            get
            {
                var uriBuilder = new StringBuilder();

                if (!string.IsNullOrEmpty(BaseUri))
                {
                    uriBuilder.Append(BaseUri.EndsWith("/") ? BaseUri : $"{BaseUri}/");
                }

                if (!string.IsNullOrEmpty(Endpoint))
                {
                    uriBuilder.Append(Endpoint.StartsWith("/") ? Endpoint.Skip(1) : Endpoint);
                }

                return uriBuilder.ToString();
            }
        }
    }
}
