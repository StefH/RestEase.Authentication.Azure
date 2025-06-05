using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestEase;
using RestEase.Authentication.Azure;
using RestEase.Authentication.Azure.Authentication;
using RestEase.Authentication.Azure.Http;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;
using RestEase.Authentication.Azure.RetryPolicies;
using RestEase.HttpClientFactory;
using Stef.Validation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseWithAzureAuthenticatedRestEaseClient<T>(
        this IServiceCollection services,
        IConfigurationSection section,
        Action<RestClient>? configureRestClient = null) where T : class
    {
        Guard.NotNull(services);
        Guard.NotNull(section);

        var options = new AzureAuthenticatedRestEaseOptions<T>();
        section.Bind(options);

        return services.UseWithAzureAuthenticatedRestEaseClient(options, configureRestClient);
    }

    public static IServiceCollection UseWithAzureAuthenticatedRestEaseClient<T>(
        this IServiceCollection services,
        Action<AzureAuthenticatedRestEaseOptions<T>> configureAction,
        Action<RestClient>? configureRestClient = null) where T : class
    {
        Guard.NotNull(services);
        Guard.NotNull(configureAction);

        var options = new AzureAuthenticatedRestEaseOptions<T>();
        configureAction(options);

        return services.UseWithAzureAuthenticatedRestEaseClient(options, configureRestClient);
    }

    public static IServiceCollection UseWithAzureAuthenticatedRestEaseClient<T>(
        this IServiceCollection services,
        AzureAuthenticatedRestEaseOptions<T> options,
        Action<RestClient>? configureRestClient = null) where T : class
    {
        Guard.NotNull(services);
        Guard.NotNull(options);

        if (string.IsNullOrEmpty(options.HttpClientName))
        {
            options.HttpClientName = typeof(T).FullName;
        }

        if (string.IsNullOrEmpty(options.AccessTokenCacheKeyPrefix))
        {
            options.AccessTokenCacheKeyPrefix = typeof(T).FullName;
        }

        if (options.LogRequest || options.LogResponse)
        {
            services.AddTransient<CustomHttpLoggingHandler<T>>();
        }

        // Azure services
        services.AddSingleton<ITokenCredentialFactory<T>, TokenCredentialFactory<T>>();
        services.AddSingleton<IAccessTokenService<T>, AccessTokenService<T>>();

        // HttpClient and RestEase services
        services
            .AddTransient<AuthenticationHttpMessageHandler<T>>()
            .AddTransient<CustomHttpClientHandler<T>>()
            .AddHttpClient(options.HttpClientName!, httpClient =>
            {
                httpClient.BaseAddress = options.BaseAddress;
                httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutInSeconds);
            })
            .ConfigurePrimaryHttpMessageHandler<CustomHttpClientHandler<T>>()
            .AddCustomHttpLoggingHandler(options)
            .AddHttpMessageHandler<AuthenticationHttpMessageHandler<T>>()
            .AddPolicyHandler((serviceProvider, _) => HttpClientRetryPolicies.GetPolicy<T>(serviceProvider))
            .UseWithRestEaseClient<T>(config =>
            {
                configureRestClient?.Invoke(config);

                if (config.JsonSerializerSettings != null && options.WriteJsonIndented != null)
                {
                    config.JsonSerializerSettings.Formatting = options.WriteJsonIndented.Value ? Formatting.Indented : Formatting.None;
                }
            });

        services.AddOptionsWithDataAnnotationValidation(options);

        services.AddMemoryCache();

        return services;
    }
}