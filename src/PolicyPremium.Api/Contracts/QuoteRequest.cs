using System.ComponentModel.DataAnnotations;
using PolicyPremium.Api.Domain;

namespace PolicyPremium.Api.Contracts;

/// <summary>
/// Incoming quote request.
///
/// Coverage and Region are accepted as strings so an unsupported value produces a clear,
/// field-keyed validation message rather than an opaque JSON-deserialization failure. The
/// data-annotation attributes both enforce the rules at runtime (returning an RFC 9457
/// validation problem) and flow into the generated OpenAPI schema — so Swagger advertises the
/// permitted enum values and numeric bounds. <c>nameof</c> keeps the allowed strings tied to
/// the <see cref="CoverageType"/> / <see cref="Region"/> enum members.
/// </summary>
public record QuoteRequest(
    [property: Required]
    [property: AllowedValues(
        nameof(CoverageType.Basic),
        nameof(CoverageType.Standard),
        nameof(CoverageType.Comprehensive),
        ErrorMessage = "Unsupported coverage type. Supported values: Basic, Standard, Comprehensive.")]
    string Coverage,
    [property: Required]
    [property: AllowedValues(
        nameof(Region.LowRisk),
        nameof(Region.Standard),
        nameof(Region.Urban),
        nameof(Region.Coastal),
        ErrorMessage = "Unsupported region. Supported values: LowRisk, Standard, Urban, Coastal.")]
    string Region,
    [property: Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Sum insured must be greater than 0.")]
    decimal SumInsured,
    [property: Range(0, int.MaxValue, ErrorMessage = "Prior claims count must be 0 or greater.")]
    int PriorClaims);
