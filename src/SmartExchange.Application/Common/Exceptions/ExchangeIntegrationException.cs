namespace SmartExchange.Application.Common.Exceptions;

public class ExchangeIntegrationException : Exception
{
    public ExchangeIntegrationException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
