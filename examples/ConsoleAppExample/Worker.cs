using System.Text.Json;
using ConsoleAppExample.Api;
using ConsoleAppExample.Models;
using Microsoft.Extensions.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ConsoleAppExample;

internal class Worker
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    private readonly IDocumentApi _documentApi;
    private readonly ILogger<Worker> _logger;

    public Worker(IDocumentApi documentApi, ILogger<Worker> logger)
    {
        _documentApi = documentApi;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var server = WireMockServer.Start("https://localhost:44318");

        server
            .Given(
                Request.Create().UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("ContentType", "application/json")
                    .WithBodyAsJson(new Document
                    {
                        Id = 1234,
                        Title = "_t_",
                        Description = "{{ request.headers.Authorization }}"
                    })
                    .WithTransformer()
            );

        try
        {
            var docA = await _documentApi.GetDocumentAsync(1, cancellationToken);
            _logger.LogInformation("IDocumentApi : GetDocumentAsync = '{doc}'", JsonSerializer.Serialize(docA.GetContent(), _options));

            await Task.Delay(500, cancellationToken);

            var docB = await _documentApi.GetDocumentAsync(2, cancellationToken);
            _logger.LogInformation("IDocumentApi : GetDocumentAsync = '{doc}'", JsonSerializer.Serialize(docB.GetContent(), _options));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}