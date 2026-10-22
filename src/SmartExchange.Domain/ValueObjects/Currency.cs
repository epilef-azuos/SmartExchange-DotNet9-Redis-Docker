using SmartExchange.Domain.Common;

namespace SmartExchange.Domain.ValueObjects;

// Sealed record (classe) em vez de readonly record struct: uma struct aceitaria
// default(Currency) com Code nulo, contornando a validação do construtor.
public sealed record Currency
{
    public Currency(string code)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();

        if (normalized.Length != 3 || !normalized.All(c => c is >= 'A' and <= 'Z'))
        {
            throw new DomainException($"Código de moeda inválido: '{code}'. Use 3 letras ISO 4217.");
        }

        Code = normalized;
    }

    public string Code { get; }

    public override string ToString() => Code;
}
