using FluentAssertions;
using PolicyPremium.Api.Domain;

namespace PolicyPremium.Tests;

/// <summary>
/// Unit tests for the pure premium calculation. Expected values are written out longhand in
/// the comments so the rule being exercised is obvious from the test.
/// </summary>
public class PremiumCalculatorTests
{
    [Fact]
    public void Calculate_BaselineQuote_AppliesOnlyTheBaseRate()
    {
        // 100,000 * 0.005 * 1.00 (Basic) * 1.00 (Standard) * 1.00 (no claims)
        var premium = PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, 0);

        premium.Should().Be(500.00m);
    }

    [Fact]
    public void Calculate_ComprehensiveCoverage_CostsMoreThanBasic()
    {
        var basic = PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, 0);
        var comprehensive = PremiumCalculator.Calculate(100_000m, CoverageType.Comprehensive, Region.Standard, 0);

        comprehensive.Should().BeGreaterThan(basic);
        comprehensive.Should().Be(750.00m); // 500 * 1.50
    }

    [Fact]
    public void Calculate_UrbanAndCoastalRegions_CostMoreThanStandardRegion()
    {
        var standard = PremiumCalculator.Calculate(100_000m, CoverageType.Standard, Region.Standard, 0);
        var urban = PremiumCalculator.Calculate(100_000m, CoverageType.Standard, Region.Urban, 0);
        var coastal = PremiumCalculator.Calculate(100_000m, CoverageType.Standard, Region.Coastal, 0);

        standard.Should().Be(625.00m); // 100,000 * 0.005 * 1.25
        urban.Should().Be(718.75m); // 625 * 1.15
        coastal.Should().Be(750.00m); // 625 * 1.20

        urban.Should().BeGreaterThan(standard);
        coastal.Should().BeGreaterThan(urban);
    }

    [Fact]
    public void Calculate_PriorClaims_IncreaseThePremium()
    {
        var noClaims = PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, 0);
        var twoClaims = PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, 2);

        twoClaims.Should().BeGreaterThan(noClaims);
        twoClaims.Should().Be(600.00m); // 500 * 1.20 (+10% per claim)
    }

    [Fact]
    public void Calculate_ClaimsLoading_IsCappedAtFiftyPercent()
    {
        var fiveClaims = PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, 5);
        var tenClaims = PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, 10);

        fiveClaims.Should().Be(750.00m); // 500 * 1.50 (capped at +50%)
        tenClaims.Should().Be(fiveClaims); // further claims do not raise the premium
    }

    [Fact]
    public void Calculate_WhenComputedPremiumIsBelowFloor_ReturnsMinimumPremium()
    {
        // 1,000 * 0.005 = 5.00, well below the 100.00 minimum.
        var premium = PremiumCalculator.Calculate(1_000m, CoverageType.Basic, Region.Standard, 0);

        premium.Should().Be(100.00m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Calculate_InvalidSumInsured_IsRejected(int sumInsured)
    {
        var act = () => PremiumCalculator.Calculate(sumInsured, CoverageType.Basic, Region.Standard, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Calculate_NegativeClaimsCount_IsRejected()
    {
        var act = () => PremiumCalculator.Calculate(100_000m, CoverageType.Basic, Region.Standard, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
