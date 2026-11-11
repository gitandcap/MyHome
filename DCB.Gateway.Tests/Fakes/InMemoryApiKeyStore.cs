using DCB.Gateway.Abstractions;

namespace DCB.Gateway.Tests.Fakes;

/// <summary><see cref="IApiKeyStore"/> in memoria, al posto del file <c>apikey.json</c>.</summary>
internal sealed class InMemoryApiKeyStore : IApiKeyStore
{
    public InMemoryApiKeyStore(string? apiKey = null)
    {
        ApiKey = apiKey;
    }

    public string? ApiKey { get; private set; }

    public DateTimeOffset? ObtainedAt { get; private set; }

    public int SaveCount { get; private set; }

    public Task<string?> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(ApiKey);

    public Task SaveAsync(string apiKey, DateTimeOffset obtainedAt, CancellationToken cancellationToken)
    {
        ApiKey = apiKey;
        ObtainedAt = obtainedAt;
        SaveCount++;
        return Task.CompletedTask;
    }
}
