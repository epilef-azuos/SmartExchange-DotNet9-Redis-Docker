using MediatR;
using SmartExchange.Domain.Models;

namespace SmartExchange.Application.Exchange.Events;

public sealed record ConversionExecutedNotification(ConversionResult Result) : INotification;
