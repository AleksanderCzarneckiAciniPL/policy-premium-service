namespace PolicyPremium.Api.Contracts;

/// <summary>
/// Incoming quote request. Coverage and Region are strings so unsupported values can be
/// rejected with a clear validation message rather than a deserialization failure.
/// </summary>
public record QuoteRequest(
    string Coverage,
    string Region,
    decimal SumInsured,
    int PriorClaims);
