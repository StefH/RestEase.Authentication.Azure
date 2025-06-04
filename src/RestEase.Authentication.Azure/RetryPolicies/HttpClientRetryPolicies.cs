using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace RestEase.Authentication.Azure.RetryPolicies;

internal static class HttpClientRetryPolicies
{
    private const int TotalRetryCount = 3;

    public static IAsyncPolicy<HttpResponseMessage> GetPolicy<T>(IServiceProvider serviceProvider) where T : class
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrInner<TaskCanceledException>()
            .WaitAndRetryAsync(TotalRetryCount, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)), (result, timeSpan, retryCount, _) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<T>>();
                var reason = result?.Result?.StatusCode.ToString() ?? result?.Exception.Message;

                logger.LogWarning("Request failed with '{reason}'. Waiting {timeSpan} before next retry. Retry attempt {retryCount}/{totalRetryCount}.", reason, timeSpan, retryCount, TotalRetryCount);
            });
    }
}