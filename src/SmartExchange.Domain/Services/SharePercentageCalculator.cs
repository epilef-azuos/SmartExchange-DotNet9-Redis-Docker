namespace SmartExchange.Domain.Services;

// Calcula a fatia percentual (0 a 100, 2 casas) de cada valor sobre o total.
// Usa o método dos maiores restos: as fatias somam exatamente 100,00 quando o total é positivo.
public static class SharePercentageCalculator
{
    // 100,00% expresso em centésimos de ponto percentual.
    private const long FullShareUnits = 10000;

    public static IReadOnlyList<decimal> Calculate(IReadOnlyList<decimal> amounts)
    {
        var total = amounts.Sum();

        if (total <= 0)
        {
            return amounts.Select(_ => 0m).ToList();
        }

        var units = new long[amounts.Count];
        var remainders = new decimal[amounts.Count];

        for (var i = 0; i < amounts.Count; i++)
        {
            var exact = amounts[i] * FullShareUnits / total;
            units[i] = (long)Math.Floor(exact);
            remainders[i] = exact - units[i];
        }

        var remaining = FullShareUnits - units.Sum();

        foreach (var index in Enumerable.Range(0, amounts.Count)
                     .OrderByDescending(i => remainders[i])
                     .ThenBy(i => i)
                     .Take((int)remaining))
        {
            units[index]++;
        }

        return units.Select(ToPercentage).ToList();
    }

    private static decimal ToPercentage(long units) => new((int)units, 0, 0, false, 2);
}
