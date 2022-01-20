using ConsoleAppExample.Models;
using RestEase;

namespace ConsoleAppExample.Api;

[BasePath("/api")]
public interface IDocumentApi
{
    [Get("GetDocumentById/{id}")]
    Task<Response<Document>> GetDocumentAsync([Path] int id, CancellationToken cancellationToken = default);
}