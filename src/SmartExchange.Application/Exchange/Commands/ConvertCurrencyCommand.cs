using MediatR;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Commands;

public sealed record ConvertCurrencyCommand(string From, string To, decimal Amount, string? Tier) : IRequest<ConversionResponse>;
