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
}
