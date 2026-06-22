namespace PolicyPremium.Api.Domain;

/// <summary>
/// A stored premium quote: the validated inputs plus the calculated premium.
/// </summary>
public record Quote(
    Guid Id,
    CoverageType Coverage,
    Region Region,
    decimal SumInsured,
    int PriorClaims,
    decimal Premium,
    DateTimeOffset CreatedAt);
