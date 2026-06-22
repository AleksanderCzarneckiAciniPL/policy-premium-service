using PolicyPremium.Api.Domain;

namespace PolicyPremium.Api.Contracts;

/// <summary>
/// Quote returned to the caller, echoing the normalized inputs and the calculated premium.
/// </summary>
public record QuoteResponse(
    Guid Id,
    string Coverage,
    string Region,
    decimal SumInsured,
    int PriorClaims,
    decimal Premium,
    DateTimeOffset CreatedAt)
{
    public static QuoteResponse FromQuote(Quote quote) => new(
        quote.Id,
        quote.Coverage.ToString(),
        quote.Region.ToString(),
        quote.SumInsured,
        quote.PriorClaims,
        quote.Premium,
        quote.CreatedAt);
}
