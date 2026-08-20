// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Repository abstraction for the WorkforceSafety read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.WorkforceSafety.Models;

namespace PlantSafety.Application.Operational.WorkforceSafety.Repositories;

/// <summary>Read-only repository for WorkforceSafety data. Implemented in the Infrastructure layer.</summary>
public interface IWorkforceSafetyRepository
{
    /// <summary>Returns WorkforceSafety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkforceSafetyItemDto>> GetWorkforceSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}