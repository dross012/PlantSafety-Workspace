// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the ShiftFatigue read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.ShiftFatigue.Models;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Repositories;

/// <summary>Read-only repository for ShiftFatigue data. Implemented in the Infrastructure layer.</summary>
public interface IShiftFatigueRepository
{
    /// <summary>
    /// Returns the ShiftFatigue read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<ShiftFatigueItemDto?> GetByIdAsync(Guid shiftFatigueId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns ShiftFatigue items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ShiftFatigueItemDto>> GetShiftFatigueAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}