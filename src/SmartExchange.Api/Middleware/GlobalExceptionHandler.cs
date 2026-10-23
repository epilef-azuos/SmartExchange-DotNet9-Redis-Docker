using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Domain.Common;

namespace SmartExchange.Api.Middleware;

// Converte exceções em respostas RFC 7807 (Problem Details) com o código HTTP semântico.
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const string InternalErrorDetail = "Ocorreu um erro inesperado ao processar a requisição.";

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição malformada", exception.Message),
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Dados inválidos", exception.Message),
            ExchangeIntegrationException => (StatusCodes.Status502BadGateway, "Falha na integração com o provedor de câmbio", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor", InternalErrorDetail)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }
}
