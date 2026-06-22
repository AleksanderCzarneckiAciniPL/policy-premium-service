using PolicyPremium.Api.Contracts;
using PolicyPremium.Api.Domain;
using PolicyPremium.Api.Storage;
using PolicyPremium.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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
    .WithName("Health");

app.MapPost("/quotes", (QuoteRequest request, IQuoteRepository repository) =>
{
    if (!QuoteRequestValidator.TryValidate(request, out var coverage, out var region, out var errors))
    {
        return Results.ValidationProblem(errors);
    }

    var premium = PremiumCalculator.Calculate(request.SumInsured, coverage, region, request.PriorClaims);

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
.WithName("CreateQuote");

app.MapGet("/quotes/{id:guid}", (Guid id, IQuoteRepository repository) =>
{
    var quote = repository.GetById(id);
    return quote is null
        ? Results.NotFound()
        : Results.Ok(QuoteResponse.FromQuote(quote));
})
.WithName("GetQuote");

app.Run();

/// <summary>
/// Exposed so <c>WebApplicationFactory</c> can host the app for integration tests.
/// </summary>
public partial class Program;
