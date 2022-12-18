namespace RestEase.Authentication.Azure.Interfaces;

// ReSharper disable once UnusedTypeParameter
public interface IAccessTokenService<T> where T : class
{
    Task<string> GetTokenAsync(string resource, CancellationToken cancellationToken = default);

    Task<string> GetTokenAsync(string resource, bool forceRefresh, CancellationToken cancellationToken = default);
}