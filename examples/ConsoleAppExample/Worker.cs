using ConsoleAppExample.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ConsoleAppExample;

internal class Worker
{
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
                    //.WithBodyAsJson(new Document
                    //{
                    //    Id = 1234,
                    //    Title = "_t_",
                    //    Description = "{{ request.headers.Authorization }}"
                    //})
                    .WithBodyAsJson(new
                    {
                        id = 1234,
                        x = 42,
                        Title = "_t_",
                        Description = "{{ request.headers.Authorization }}"
                    })
                    .WithTransformer()
            );

        try
        {
            var doc1 = await _documentApi.GetDocumentAsync(1, cancellationToken);
            _logger.LogInformation("IDocumentApi : GetDocumentAsync = '{doc}'", JsonConvert.SerializeObject(doc1.CurrentValue, Formatting.Indented));

            await Task.Delay(1000, cancellationToken);

            var doc2 = await _documentApi.GetDocumentAsync(2, cancellationToken);
            _logger.LogInformation("IDocumentApi : GetDocumentAsync = '{doc}'", JsonConvert.SerializeObject(doc2.CurrentValue, Formatting.Indented));

            await Task.Delay(1000, cancellationToken);

            var doc3 = await _documentApi.GetDocumentAsync(3, cancellationToken);
            _logger.LogInformation("IDocumentApi : GetDocumentAsync = '{doc}'", JsonConvert.SerializeObject(doc3.CurrentValue, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}