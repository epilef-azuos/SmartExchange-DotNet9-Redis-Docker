using MediatR;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Application.Exchange.Events;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.Strategies;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Application.Exchange.Commands;

public sealed class ConvertCurrencyCommandHandler : IRequestHandler<ConvertCurrencyCommand, ConversionResponse>
{
    private readonly IExchangeRateProvider _rateProvider;
    private readonly ISpreadStrategyResolver _spreadResolver;
    private readonly IPublisher _publisher;

    public ConvertCurrencyCommandHandler(
        IExchangeRateProvider rateProvider,
        ISpreadStrategyResolver spreadResolver,
        IPublisher publisher)
    {
        _rateProvider = rateProvider;
        _spreadResolver = spreadResolver;
        _publisher = publisher;
    }

    public async Task<ConversionResponse> Handle(ConvertCurrencyCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ValidationException("O montante deve ser maior que zero.");
        }

        var tier = ClientTierParser.Parse(request.Tier);
        var source = new Money(request.Amount, new Currency(request.From));
        var target = new Currency(request.To);

        if (source.Currency == target)
        {
            throw new DomainException("A moeda de origem e a de destino devem ser diferentes.");
        }

        var rate = await _rateProvider.GetLatestRateAsync(source.Currency, target, cancellationToken);
        var strategy = _spreadResolver.Resolve(tier);
        var result = ConversionResult.Calculate(source, rate, strategy);

        await _publisher.Publish(new ConversionExecutedNotification(result), cancellationToken);

        return ConversionResponse.From(result);
    }
}
