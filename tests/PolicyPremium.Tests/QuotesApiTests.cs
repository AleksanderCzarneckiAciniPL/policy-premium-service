using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PolicyPremium.Api.Contracts;

namespace PolicyPremium.Tests;

/// <summary>
/// API/integration tests that exercise the real HTTP pipeline (routing, validation,
/// serialization and the in-memory store) via <see cref="WebApplicationFactory{T}"/>.
/// </summary>
public class QuotesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public QuotesApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static object ValidRequest() => new
    {
        coverage = "Comprehensive",
        region = "Coastal",
        sumInsured = 100_000m,
        priorClaims = 2,
    };

    [Fact]
    public async Task PostQuote_WithValidRequest_Returns201Created()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/quotes", ValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostQuote_ReturnsQuoteIdAndPremium()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/quotes", ValidRequest());
        var quote = await response.Content.ReadFromJsonAsync<QuoteResponse>();

        quote.Should().NotBeNull();
        quote!.Id.Should().NotBeEmpty();
        // 100,000 * 0.005 * 1.50 (Comprehensive) * 1.20 (Coastal) * 1.20 (2 claims)
        quote.Premium.Should().Be(1080.00m);
    }

    [Fact]
    public async Task GetQuote_AfterCreate_ReturnsTheSameQuote()
    {
        var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/quotes", ValidRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<QuoteResponse>();

        var getResponse = await client.GetAsync($"/quotes/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content.ReadFromJsonAsync<QuoteResponse>();
        fetched.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GetQuote_WithUnknownId_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/quotes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostQuote_WithInvalidRequest_Returns400()
    {
        var client = _factory.CreateClient();
        var invalid = new
        {
            coverage = "Platinum", // not a supported coverage type
            region = "Coastal",
            sumInsured = 100_000m,
            priorClaims = 0,
        };

        var response = await client.PostAsJsonAsync("/quotes", invalid);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
