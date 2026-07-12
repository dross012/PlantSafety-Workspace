// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Auditing read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Auditing.Models;

namespace PlantSafety.Application.Integrations.Auditing.Repositories;

/// <summary>Read-only repository for Integrations.Auditing data. Implemented in the Infrastructure layer.</summary>
public interface IAuditingRepository
{
    /// <summary>Returns Integrations.Auditing items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AuditingItemDto>> GetLogsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
