using PolicyPremium.Api.Contracts;
using PolicyPremium.Api.Domain;
using PolicyPremium.Api.Storage;

namespace PolicyPremium.Api.Endpoints;

/// <summary>
/// Quote creation and retrieval endpoints.
/// </summary>
public static class QuoteEndpoints
{
    public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        var quotes = app.MapGroup("/quotes").WithTags("Quotes");

        quotes.MapPost("/", CreateQuote)
            .WithName("CreateQuote")
            .WithSummary("Create a premium quote")
            .WithDescription(
                "Calculates a premium from the supplied risk inputs and stores the resulting quote.\n\n" +
                "Multipliers: coverage (Basic 1.00, Standard 1.25, Comprehensive 1.50), " +
                "region (LowRisk 0.90, Standard 1.00, Urban 1.15, Coastal 1.20), " +
                "claims (+10% per prior claim, capped at +50%). A minimum premium of 100.00 always applies.")
            .Produces<QuoteResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        quotes.MapGet("/{id:guid}", GetQuote)
            .WithName("GetQuote")
            .WithSummary("Retrieve a quote by id")
            .WithDescription("Returns a previously created quote. Responds with 404 if no quote exists for the supplied id.")
            .Produces<QuoteResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static IResult CreateQuote(QuoteRequest request, IQuoteRepository repository)
    {
        // Validation has already guaranteed these are permitted enum names (any case).
        var coverage = Enum.Parse<CoverageType>(request.Coverage, ignoreCase: true);
        var region = Enum.Parse<Region>(request.Region, ignoreCase: true);

        var premium = PremiumCalculator.Calculate(
            request.SumInsured, coverage, region, request.PriorClaims);

        var quote = new Quote(
            Guid.NewGuid(),
            coverage,
            region,
            request.SumInsured,
            request.PriorClaims,
            premium,
            DateTimeOffset.UtcNow);

        repository.Add(quote);

        return Results.Created($"/quotes/{quote.Id}", QuoteResponse.FromQuote(quote));
    }

    private static IResult GetQuote(Guid id, IQuoteRepository repository)
    {
        var quote = repository.GetById(id);
        return quote is null
            ? Results.NotFound()
            : Results.Ok(QuoteResponse.FromQuote(quote));
    }
}
