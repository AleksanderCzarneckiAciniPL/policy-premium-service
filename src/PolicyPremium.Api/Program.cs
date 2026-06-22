using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using PolicyPremium.Api.Contracts;
using PolicyPremium.Api.Domain;
using PolicyPremium.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

// Validate request DTOs from their data-annotation attributes and return an RFC 9457
// validation problem on failure. Enabled for minimal APIs in .NET 10.
builder.Services.AddValidation();

// Render framework-level errors (e.g. a body that can't be bound to the schema) as RFC 9457
// problem responses rather than bare status codes.
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    // Document-level metadata shown at the top of Swagger UI.
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Policy Premium API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Calculates and stores insurance policy premium quotes.\n\n" +
            "Premium = max(100, sumInsured × 0.005 × coverageMultiplier × regionMultiplier × claimsMultiplier), " +
            "rounded to two decimal places. Quotes are held in memory only and are lost on restart.";
        return Task.CompletedTask;
    });

    // [Range], [Required] etc. flow into the schema automatically, but [AllowedValues] does not.
    // Project it onto the schema's `enum` so Swagger advertises the permitted values as a list.
    options.AddSchemaTransformer((schema, context, _) =>
    {
        var allowed = context.JsonPropertyInfo?.AttributeProvider?
            .GetCustomAttributes(typeof(AllowedValuesAttribute), inherit: false)
            .Cast<AllowedValuesAttribute>()
            .FirstOrDefault();

        if (allowed is not null)
        {
            schema.Enum = allowed.Values
                .Where(value => value is not null)
                .Select(value => JsonValue.Create(value!.ToString()) as JsonNode)
                .ToList()!;
        }

        return Task.CompletedTask;
    });
});
builder.Services.AddSingleton<IQuoteRepository, InMemoryQuoteRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Policy Premium API v1");
        options.DocumentTitle = "Policy Premium API";
    });
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithName("Health")
    .WithTags("Diagnostics")
    .WithSummary("Liveness check")
    .WithDescription("Returns 200 with a status payload while the service is running. Used by orchestrators and uptime checks.")
    .Produces(StatusCodes.Status200OK);

app.MapPost("/quotes", (QuoteRequest request, IQuoteRepository repository) =>
{
    // Validation (above) has already guaranteed these are permitted enum names.
    var coverage = Enum.Parse<CoverageType>(request.Coverage);
    var region = Enum.Parse<Region>(request.Region);

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

    var response = QuoteResponse.FromQuote(quote);
    return Results.Created($"/quotes/{quote.Id}", response);
})
.WithName("CreateQuote")
.WithTags("Quotes")
.WithSummary("Create a premium quote")
.WithDescription(
    "Calculates a premium from the supplied risk inputs and stores the resulting quote.\n\n" +
    "Multipliers: coverage (Basic 1.00, Standard 1.25, Comprehensive 1.50), " +
    "region (LowRisk 0.90, Standard 1.00, Urban 1.15, Coastal 1.20), " +
    "claims (+10% per prior claim, capped at +50%). A minimum premium of 100.00 always applies.")
.Produces<QuoteResponse>(StatusCodes.Status201Created)
.ProducesValidationProblem();

app.MapGet("/quotes/{id:guid}", (Guid id, IQuoteRepository repository) =>
{
    var quote = repository.GetById(id);
    return quote is null
        ? Results.NotFound()
        : Results.Ok(QuoteResponse.FromQuote(quote));
})
.WithName("GetQuote")
.WithTags("Quotes")
.WithSummary("Retrieve a quote by id")
.WithDescription("Returns a previously created quote. Responds with 404 if no quote exists for the supplied id.")
.Produces<QuoteResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.Run();

/// <summary>
/// Exposed so <c>WebApplicationFactory</c> can host the app for integration tests.
/// </summary>
public partial class Program;
