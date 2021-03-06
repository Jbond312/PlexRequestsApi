﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PlexRequests.Functions
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> DeserializeAndValidateRequest<T>(this HttpRequest req) where T : class
        {
            return await Task.Run(() =>
            {
                using var reader = new StreamReader(req.Body);
                using var jsonReader = new JsonTextReader(reader);
                return new JsonSerializer().Deserialize<T>(jsonReader);
            });
        }
    }
}
