namespace PolicyPremium.Api.Domain;

/// <summary>
/// Pure, deterministic premium calculation. No I/O, no state — easy to unit test.
///
/// Rules (see CLAUDE.md / README):
///   base premium    = sumInsured * 0.005
///   coverage factor = Basic 1.00, Standard 1.25, Comprehensive 1.50
///   region factor   = LowRisk 0.90, Standard 1.00, Urban 1.15, Coastal 1.20
///   claims loading  = +10% per prior claim, capped at +50%
///   minimum premium = 100.00
///   final premium is rounded to two decimal places.
///
/// Rejects a non-positive sum insured or a negative claims count with
/// <see cref="ArgumentOutOfRangeException"/>.
/// </summary>
public static class PremiumCalculator
{
    private const decimal BaseRate = 0.005m;
    private const decimal ClaimLoadingPerClaim = 0.10m;
    private const decimal MaxClaimLoading = 0.50m;
    private const decimal MinimumPremium = 100.00m;

    public static decimal Calculate(decimal sumInsured, CoverageType coverage, Region region, int priorClaims)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sumInsured);
        ArgumentOutOfRangeException.ThrowIfNegative(priorClaims);

        var basePremium = sumInsured * BaseRate;
        var coverageFactor = CoverageFactor(coverage);
        var regionFactor = RegionFactor(region);
        var claimsLoading = ClaimsLoading(priorClaims);

        var premium = basePremium * coverageFactor * regionFactor * (1m + claimsLoading);

        if (premium < MinimumPremium)
        {
            premium = MinimumPremium;
        }

        return Math.Round(premium, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal ClaimsLoading(int priorClaims)
    {
        var loading = priorClaims * ClaimLoadingPerClaim;
        return loading > MaxClaimLoading ? MaxClaimLoading : loading;
    }

    private static decimal CoverageFactor(CoverageType coverage) => coverage switch
    {
        CoverageType.Basic => 1.00m,
        CoverageType.Standard => 1.25m,
        CoverageType.Comprehensive => 1.50m,
        _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "Unsupported coverage type.")
    };

    private static decimal RegionFactor(Region region) => region switch
    {
        Region.LowRisk => 0.90m,
        Region.Standard => 1.00m,
        Region.Urban => 1.15m,
        Region.Coastal => 1.20m,
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unsupported region.")
    };
}
