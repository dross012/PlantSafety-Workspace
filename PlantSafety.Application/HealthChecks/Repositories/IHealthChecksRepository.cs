// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HealthChecks read-model.
// SchemaVersion: 1

using PlantSafety.Application.HealthChecks.Models;

namespace PlantSafety.Application.HealthChecks.Repositories;

/// <summary>Read-only repository for HealthChecks data. Implemented in the Infrastructure layer.</summary>
public interface IHealthChecksRepository
{
    /// <summary>Returns HealthChecks items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HealthCheckItemDto>> GetChecksAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
