using SmartExchange.Domain.Common;

namespace SmartExchange.Domain.ValueObjects;

public sealed record Money
{
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new DomainException("O valor monetário não pode ser negativo.");
        }

        Amount = amount;
        Currency = currency ?? throw new DomainException("A moeda é obrigatória.");
    }

    public decimal Amount { get; }

    public Currency Currency { get; }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
        {
            throw new DomainException("Divisão de valor monetário por zero não é permitida.");
        }

        return new Money(Amount / divisor, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException(
                $"Operação inválida entre moedas diferentes: {Currency} e {other.Currency}.");
        }
    }
}
