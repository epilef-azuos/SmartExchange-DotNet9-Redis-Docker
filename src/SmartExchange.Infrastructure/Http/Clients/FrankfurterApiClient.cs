using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.ValueObjects;
using SmartExchange.Infrastructure.Http.Dtos;

namespace SmartExchange.Infrastructure.Http.Clients;

public sealed class FrankfurterApiClient : IExchangeRateProvider
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly HttpClient _httpClient;

    public FrankfurterApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRate> GetLatestRateAsync(Currency baseCurrency, Currency targetCurrency, CancellationToken ct = default)
    {
        var response = await GetAsync<FrankfurterLatestResponse>(
            $"latest?base={baseCurrency.Code}&symbols={targetCurrency.Code}", ct);

        if (!response.Rates.TryGetValue(targetCurrency.Code, out var rate))
        {
            throw new ExchangeIntegrationException($"A resposta não contém a taxa para {targetCurrency.Code}.");
        }

        var timestampUtc = new DateTimeOffset(response.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        try
        {
            return new ExchangeRate(baseCurrency, targetCurrency, rate, timestampUtc);
        }
        catch (DomainException ex)
        {
            throw new ExchangeIntegrationException("A taxa retornada pela Frankfurter API é inválida.", null, ex);
        }
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetLatestRatesAsync(Currency baseCurrency, CancellationToken ct = default)
    {
        var response = await GetAsync<FrankfurterLatestResponse>($"latest?base={baseCurrency.Code}", ct);

        return response.Rates;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        return await GetAsync<Dictionary<string, string>>("currencies", ct);
    }

    public async Task<IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>>> GetHistoricalRatesAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default)
    {
        var response = await GetAsync<FrankfurterTimeSeriesResponse>(
            $"{startDate.ToString(DateFormat, CultureInfo.InvariantCulture)}..{endDate.ToString(DateFormat, CultureInfo.InvariantCulture)}" +
            $"?base={baseCurrency.Code}&symbols={targetCurrency.Code}",
            ct);

        var historical = new Dictionary<DateOnly, Dictionary<string, decimal>>();

        foreach (var (date, rates) in response.Rates)
        {
            if (!DateOnly.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                throw new ExchangeIntegrationException($"A Frankfurter API retornou uma data inválida: '{date}'.");
            }

            historical[parsedDate] = rates;
        }

        return historical;
    }

    private async Task<T> GetAsync<T>(string requestUri, CancellationToken ct) where T : class
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync(requestUri, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new ExchangeIntegrationException("Falha de comunicação com a Frankfurter API.", null, ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;

                throw new ExchangeIntegrationException(
                    $"A Frankfurter API respondeu com status {statusCode}.",
                    statusCode);
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<T>(ct)
                    ?? throw new ExchangeIntegrationException("A Frankfurter API retornou um payload vazio.");
            }
            catch (JsonException ex)
            {
                throw new ExchangeIntegrationException("A Frankfurter API retornou um payload inválido.", null, ex);
            }
        }
    }
}
