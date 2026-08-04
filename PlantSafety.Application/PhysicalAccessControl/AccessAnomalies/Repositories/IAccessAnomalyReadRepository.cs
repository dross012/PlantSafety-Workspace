// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the AccessAnomaly read-model.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Models;

namespace PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Repositories;

/// <summary>Read-only repository for AccessAnomaly data. Implemented in the Infrastructure layer.</summary>
public interface IAccessAnomalyReadRepository
{
    /// <summary>
    /// Returns anomalies for the tenant that are not yet Resolved (Detected, Acknowledged, or
    /// Escalated), optionally scoped to a single site, ordered by severity descending.
    /// </summary>
    Task<IReadOnlyList<AccessAnomalyDto>> ListActiveAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}
