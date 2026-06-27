// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Search
//  Layer:          Infrastructure
//  File:           ISearchService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstraction over full-text search operations (Azure AI Search).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Search;

/// <summary>Abstraction over full-text and faceted search (Azure AI Search).</summary>
public interface ISearchService
{
    /// <summary>Executes a full-text search and returns typed results.</summary>
    Task<IReadOnlyList<T>> SearchAsync<T>(string indexName, string query, int top = 20, CancellationToken cancellationToken = default);

    /// <summary>Indexes a document into the named index.</summary>
    Task IndexAsync<T>(string indexName, T document, CancellationToken cancellationToken = default);

    /// <summary>Removes a document from the named index by its key.</summary>
    Task DeleteIndexedAsync(string indexName, string key, CancellationToken cancellationToken = default);
}
