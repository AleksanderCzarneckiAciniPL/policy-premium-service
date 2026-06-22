using PolicyPremium.Api.Endpoints;
using PolicyPremium.Api.Extensions;
using PolicyPremium.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

// Validate request DTOs from their data-annotation attributes and return an RFC 9457
// validation problem on failure. Enabled for minimal APIs in .NET 10.
builder.Services.AddValidation();

// Render framework-level errors (e.g. a body that can't be bound to the schema) as RFC 9457
// problem responses rather than bare status codes.
builder.Services.AddProblemDetails();

builder.Services.AddPolicyPremiumOpenApi();
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

app.MapHealthEndpoints();
app.MapQuoteEndpoints();

app.Run();

/// <summary>
/// Exposed so <c>WebApplicationFactory</c> can host the app for integration tests.
/// </summary>
public partial class Program;
