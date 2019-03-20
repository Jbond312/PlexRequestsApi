using System.Collections.Generic;
using System.Net.Http;

namespace PlexRequests.Api
{
    public class ApiRequestBuilder
    {
        private readonly string _baseUri;
        private readonly string _endpoint;
        private readonly HttpMethod _httpMethod;
        private readonly Dictionary<string, string> _requestHeaders;
        private readonly Dictionary<string, string> _contentHeaders;
        public object _body { get; set; }

        public ApiRequestBuilder(string baseUri, string endpoint, HttpMethod httpMethod)
        {
            _baseUri = baseUri;
            _endpoint = endpoint;
            _httpMethod = httpMethod;
            _requestHeaders = new Dictionary<string, string>();
            _contentHeaders = new Dictionary<string, string>();
        }

        public ApiRequestBuilder AddRequestHeaders(Dictionary<string, string> headers)
        {
            AddMultipleHeaders(headers);
            return this;
        }

        public ApiRequestBuilder AddHeader(string key, string value)
        {
            AddSingleHeader(key, value);
            return this;
        }

        public ApiRequestBuilder AcceptJson()
        {
            AddHeader("Accept", "application/json");
            return this;
        }

        public ApiRequestBuilder AddJsonBody(object body)
        {
            _body = body;
            return this;
        }

        private void AddSingleHeader(string key, string value)
        {
            var headers = _requestHeaders ?? new Dictionary<string, string>();

            headers.Add(key, value);
        }

        private void AddMultipleHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                AddSingleHeader(header.Key, header.Value);
            }
        }

        public ApiRequest Build()
        {
            return new ApiRequest(_endpoint, _baseUri, _httpMethod, _requestHeaders, _contentHeaders, _body);
        }
    }
}
