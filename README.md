![Project Icon](icon.png) RestEase.Authentication.Azure
==================================

An extension to [RestEase](https://github.com/canton7/RestEase) which adds Microsoft Entra ID (Azure AD) Authentication.

Supported modes are:
- Username + Pasword
- Client with ClientSecret
- Managed Identity

## Install
[![NuGet Badge](https://img.shields.io/nuget/v/RestEase.Authentication.Azure)](https://www.nuget.org/packages/RestEase.Authentication.Azure)

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

### Configuration when using a Managed Identity with a ClientId
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

### Configuration when using a Managed Identity without a ClientId (it will fall back to DefaultAzureCredential)
#### appsettings.json
``` json
{
  "DocumentApiClientOptions": {
    "Resource": "r",
    "BaseAddress": "https://localhost:44318",
    "AcceptAnyServerCertificate": true,
    "TimeoutInSeconds": 99
  }
}
```

:bulb: If the `scopes`-array is not defined, the default the scope `{Resource}/.default` is used.

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

## 🌐 Links
- See also: [RestEase.Authentication](https://github.com/StefH/RestEase.Authentication)


## Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **RestEase.Authentication.Azure**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)