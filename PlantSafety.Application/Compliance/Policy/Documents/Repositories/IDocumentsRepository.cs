// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Documents read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Documents.Models;

namespace PlantSafety.Application.Compliance.Policy.Documents.Repositories;

/// <summary>Read-only repository for Documents data. Implemented in the Infrastructure layer.</summary>
public interface IDocumentsRepository
{
    /// <summary>Returns Documents items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DocumentsItemDto>> GetDocumentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
