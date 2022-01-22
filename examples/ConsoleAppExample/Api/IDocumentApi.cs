using AnyOfTypes;
using ConsoleAppExample.Models;
using RestEase;

namespace ConsoleAppExample.Api;

[BasePath("/api")]
public interface IDocumentApi
{
    [Get("GetDocumentById/{id}")]
    Task<AnyOf<Document, Response<object>>> GetDocumentAsync([Path] int id, CancellationToken cancellationToken = default);
}