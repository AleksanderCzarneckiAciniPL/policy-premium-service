using PolicyPremium.Api.Contracts;
using PolicyPremium.Api.Domain;

namespace PolicyPremium.Api.Validation;

/// <summary>
/// Validates and normalizes a <see cref="QuoteRequest"/>. On success it yields parsed enum
/// values ready for the calculator; on failure it returns a field-keyed error dictionary
/// shaped for <c>Results.ValidationProblem</c>.
/// </summary>
public static class QuoteRequestValidator
{
    public static bool TryValidate(
        QuoteRequest request,
        out CoverageType coverage,
        out Region region,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        coverage = default;
        region = default;

        if (!Enum.TryParse(request.Coverage, ignoreCase: true, out coverage) ||
            !Enum.IsDefined(coverage))
        {
            errors[nameof(request.Coverage)] =
                [$"Unsupported coverage type. Supported values: {string.Join(", ", Enum.GetNames<CoverageType>())}."];
        }

        if (!Enum.TryParse(request.Region, ignoreCase: true, out region) ||
            !Enum.IsDefined(region))
        {
            errors[nameof(request.Region)] =
                [$"Unsupported region. Supported values: {string.Join(", ", Enum.GetNames<Region>())}."];
        }

        if (request.SumInsured <= 0)
        {
            errors[nameof(request.SumInsured)] = ["Sum insured must be greater than 0."];
        }

        if (request.PriorClaims < 0)
        {
            errors[nameof(request.PriorClaims)] = ["Prior claims count must be 0 or greater."];
        }

        return errors.Count == 0;
    }
}
