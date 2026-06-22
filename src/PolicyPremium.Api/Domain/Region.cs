namespace PolicyPremium.Api.Domain;

/// <summary>
/// Supported rating regions. Each maps to a premium multiplier in <see cref="PremiumCalculator"/>.
/// </summary>
public enum Region
{
    LowRisk,
    Standard,
    Urban,
    Coastal
}
