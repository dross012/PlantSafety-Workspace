// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Documents read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Documents.Models;

namespace PlantSafety.Application.Integrations.Documents.Repositories;

/// <summary>Read-only repository for Integrations.Documents data. Implemented in the Infrastructure layer.</summary>
public interface IDocumentsRepository
{
    /// <summary>Returns Integrations.Documents items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DocumentsItemDto>> GetDocumentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns a paginated page of Integrations.Documents items for the tenant/site, optionally
    /// filtered by document type ("category") and/or a free-text search term matched against the
    /// file name, along with the total matching count (before paging).
    /// </summary>
    Task<(IReadOnlyList<DocumentsItemDto> Items, int TotalCount)> ListDocumentsAsync(
        Guid tenantId,
        Guid? siteId,
        string? documentType,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>Returns a single Integrations.Documents item by id, scoped to the tenant, or <see langword="null"/> when not found.</summary>
    Task<DocumentsItemDto?> GetDocumentByIdAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken);
}
