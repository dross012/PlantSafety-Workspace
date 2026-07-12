// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the KPISnapshots read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Models;

namespace PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Repositories;

/// <summary>Read-only repository for KPISnapshots data. Implemented in the Infrastructure layer.</summary>
public interface IKPISnapshotsRepository
{
    /// <summary>Returns KPISnapshots items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<KPISnapshotsItemDto>> GetKpiSnapshotsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
