using Microsoft.Extensions.DependencyInjection;
using RestEase.Authentication.Azure.Options;

namespace RestEase.Authentication.Azure.Http;

internal static class HttpClientBuilderExtensions
{
    internal static IHttpClientBuilder AddCustomHttpLoggingHandler<T>(this IHttpClientBuilder builder, AzureAuthenticatedRestEaseOptions<T> options) where T : class
    {
        if (options.LogRequest || options.LogResponse)
        {
            return builder.AddHttpMessageHandler<CustomHttpLoggingHandler<T>>();
        }

        return builder;
    }
}