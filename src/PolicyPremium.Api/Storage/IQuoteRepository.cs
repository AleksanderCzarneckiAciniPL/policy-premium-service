using PolicyPremium.Api.Domain;

namespace PolicyPremium.Api.Storage;

/// <summary>
/// Persistence abstraction for quotes. The only implementation in this slice is in-memory.
/// </summary>
public interface IQuoteRepository
{
    void Add(Quote quote);

    Quote? GetById(Guid id);
}
