using System.Collections.Concurrent;
using PolicyPremium.Api.Domain;

namespace PolicyPremium.Api.Storage;

/// <summary>
/// Thread-safe in-memory store. Registered as a singleton, so quotes live for the lifetime
/// of the process only — no database by design.
/// </summary>
public class InMemoryQuoteRepository : IQuoteRepository
{
    private readonly ConcurrentDictionary<Guid, Quote> _quotes = new();

    public void Add(Quote quote) => _quotes[quote.Id] = quote;

    public Quote? GetById(Guid id) => _quotes.TryGetValue(id, out var quote) ? quote : null;
}
