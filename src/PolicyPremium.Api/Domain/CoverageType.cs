namespace PolicyPremium.Api.Domain;

/// <summary>
/// Supported coverage levels. Each maps to a premium multiplier in <see cref="PremiumCalculator"/>.
/// </summary>
public enum CoverageType
{
    Basic,
    Standard,
    Comprehensive
}
