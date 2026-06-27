// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Search
//  Layer:          Infrastructure
//  File:           AzureSearchService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ISearchService backed by Azure AI Search SDK.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Search;

/// <summary>
/// <see cref="ISearchService"/> backed by Azure AI Search SDK.
/// Requires the <c>Azure.Search.Documents</c> NuGet package.
/// </summary>
public sealed class AzureSearchService(ILogger<AzureSearchService> logger) : ISearchService
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<T>> SearchAsync<T>(
        string indexName, string query, int top = 20, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Search query on index {Index}: {Query}", indexName, query);
        // TODO: Wire up Azure.Search.Documents SearchClient.
        return Task.FromResult<IReadOnlyList<T>>(Array.Empty<T>());
    }

    /// <inheritdoc/>
    public Task IndexAsync<T>(string indexName, T document, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Indexing document into index {Index}", indexName);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteIndexedAsync(string indexName, string key, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Deleting document {Key} from index {Index}", key, indexName);
        return Task.CompletedTask;
    }
}
