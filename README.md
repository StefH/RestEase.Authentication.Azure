![Project Icon](icon.png) RestEase.Authentication.Azure
==================================

An extension to [RestEase](https://github.com/canton7/RestEase) which adds AzureAd Authentication.

Supported modes are:
- Client with ClientSecret
- Managed Identity

## Install
[![NuGet Badge](https://buildstats.info/nuget/RestEase.Authentication.Azure)](https://www.nuget.org/packages/RestEase.Authentication.Azure)

You can install from NuGet using the following command in the package manager window:

`Install-Package RestEase.Authentication.Azure`

Or via the Visual Studio NuGet package manager or if you use the `dotnet` command:

`dotnet add package RestEase.Authentication.Azure`

### Configuration when using a ClientId + ClientSecret
#### appsettings.json
``` json
{
  "DocumentApiClientOptions": {
    "TenantId": "t",
    "ClientId": "c",
    "ClientSecret": "s",
    "Resource": "r",
    "BaseAddress": "https://localhost:44318",
    "AcceptAnyServerCertificate": true,
    "TimeoutInSeconds": 99
  }
}
```

### Configuration when using a Managed Service Identity
#### appsettings.json
``` json
{
  "DocumentApiClientOptions": {
    "ClientId": "c",
    "Resource": "r",
    "BaseAddress": "https://localhost:44318",
    "AcceptAnyServerCertificate": true,
    "TimeoutInSeconds": 99
  }
}
```

:information_source: If the `scopes` is not defined, the default the scope `{Resource}/.default` is used.

### Usage
#### :one: Create a RestEase interface
``` csharp
[BasePath("/api")]
public interface IDocumentApi
{
    [Get("GetDocumentById/{id}")]
    Task<Response<Document>> GetDocumentAsync([Path] int id, CancellationToken cancellationToken = default);
}
```

#### :two: Configure Dependency Injection

``` csharp
services.UseWithAzureAuthenticatedRestEaseClient<IDocumentApi>(configuration.GetSection("DocumentApiClientOptions"));
```

#### :three: Use it in your code
``` csharp
IDocumentApi documentApi = ...; // Injected
var document = await documentApi.GetDocumentAsync(1, cancellationToken);
```
