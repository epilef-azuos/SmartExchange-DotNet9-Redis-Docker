using System.Net;
using Moq;
using Moq.Protected;

namespace SmartExchange.Tests.Unit.Helpers;

public static class HttpMessageHandlerMock
{
    public const string BaseUrl = "https://api.frankfurter.app/";

    // Retorna as respostas na ordem informada; a última é repetida quando a fila se esgota.
    public static Mock<HttpMessageHandler> Create(params (HttpStatusCode StatusCode, string Content)[] responses)
    {
        var queue = new Queue<(HttpStatusCode StatusCode, string Content)>(responses);
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var next = queue.Count > 1 ? queue.Dequeue() : queue.Peek();

                return new HttpResponseMessage(next.StatusCode) { Content = new StringContent(next.Content) };
            });

        return handler;
    }

    public static HttpClient CreateClient(Mock<HttpMessageHandler> handler)
    {
        return new HttpClient(handler.Object) { BaseAddress = new Uri(BaseUrl) };
    }
}
