using Azure.Core;

namespace RestEase.Authentication.Azure.Interfaces;

// ReSharper disable once UnusedTypeParameter
public interface ITokenCredentialFactory<T> where T : class
{
    TokenCredential CreateCredential();
}