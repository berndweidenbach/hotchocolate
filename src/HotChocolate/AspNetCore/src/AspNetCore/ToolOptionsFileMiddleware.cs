using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using HttpRequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;

namespace HotChocolate.AspNetCore
{
    /// <summary>
    /// This middleware handles the Banana Cake Pop configuration file request.
    /// </summary>
    public class ToolOptionsFileMiddleware
    {
        private const string _configFile = "/bcp-config.json";
        private readonly HttpRequestDelegate _next;
        private readonly PathString _matchUrl;

        public ToolOptionsFileMiddleware(HttpRequestDelegate next, PathString matchUrl)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _matchUrl = matchUrl;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.IsGetOrHeadMethod() &&
                context.Request.TryMatchPath(_matchUrl, false, out PathString subPath) &&
                subPath.Value == _configFile &&
                (context.GetGraphQLToolOptions()?.Enable ?? true))
            {
                GraphQLToolOptions? options = context.GetGraphQLToolOptions();
                var config = new BananaCakePopConfiguration();

                if (options is not null)
                {
                    config.Document = options.Document;
                    config.Credentials = ConvertCredentialsToString(options.Credentials);
                    config.HttpHeaders = ConvertHttpHeadersToDictionary(options.HttpHeaders);
                    config.HttpMethod = ConvertHttpMethodToString(options.HttpMethod);
                    config.GaTrackingId = options.GaTrackingId;
                }

                await context.Response.WriteAsJsonAsync(config, context.RequestAborted);
            }
            else
            {
                await _next(context);
            }
        }

        private static string? ConvertCredentialsToString(DefaultCredentials? credentials)
        {
            if (credentials is not null)
            {
                switch (credentials)
                {
                    case DefaultCredentials.Include:
                        return "include";
                    case DefaultCredentials.Omit:
                        return "omit";
                    case DefaultCredentials.SameOrigin:
                        return "same-origin";
                }
            }

            return null;
        }

        private static IDictionary<string, string>? ConvertHttpHeadersToDictionary(
            IHeaderDictionary? httpHeaders)
        {
            if (httpHeaders is not null)
            {
                var result = new Dictionary<string, string>();

                foreach ((var key, StringValues value) in httpHeaders)
                {
                    result.Add(key, value.ToString());
                }

                return result;
            }

            return null;
        }

        private string? ConvertHttpMethodToString(DefaultHttpMethod? httpMethod)
        {
            if (httpMethod is not null)
            {
                switch (httpMethod)
                {
                    case DefaultHttpMethod.Get:
                        return "GET";
                    case DefaultHttpMethod.Post:
                        return "POST";
                }
            }

            return null;
        }

        private class BananaCakePopConfiguration
        {
            public bool UseBrowserUrlAsEndpoint { get; } = true;

            public bool? EndpointEditable { get; set; }

            public string? Document { get; set; }

            public string? Credentials { get; set; }

            public IDictionary<string, string>? HttpHeaders { get; set; }

            public string? HttpMethod { get; set; }

            public string? GaTrackingId { get; set; }
        }
    }
}
