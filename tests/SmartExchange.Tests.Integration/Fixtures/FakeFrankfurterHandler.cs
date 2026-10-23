using System.Net;
using System.Text;

namespace SmartExchange.Tests.Integration.Fixtures;

// Substitui a Frankfurter API real por respostas determinísticas, mantendo o restante do pipeline
// (serialização, resiliência, cache e persistência) real.
public sealed class FakeFrankfurterHandler : HttpMessageHandler
{
    private const string LatestUsdBrl = """{"amount":1.0,"base":"USD","date":"2026-10-22","rates":{"BRL":5.0}}""";
    private const string LatestEurUsd = """{"amount":1.0,"base":"EUR","date":"2026-10-22","rates":{"USD":1.1}}""";
    private const string Currencies = """{"BRL":"Brazilian Real","EUR":"Euro","USD":"United States Dollar"}""";

    public int RequestCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;

        var uri = request.RequestUri!;
        var body = (uri.AbsolutePath, uri.Query) switch
        {
            ("/latest", "?base=USD&symbols=BRL") => LatestUsdBrl,
            ("/latest", "?base=EUR&symbols=USD") => LatestEurUsd,
            ("/currencies", _) => Currencies,
            _ => null
        };

        var response = body is null
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

        return Task.FromResult(response);
    }
}
