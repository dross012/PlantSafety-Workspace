// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the CraneHoistSystems read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

/// <summary>Read-only repository for CraneHoistSystems data. Implemented in the Infrastructure layer.</summary>
public interface ICraneHoistSystemsRepository
{
    /// <summary>Returns CraneHoistSystems items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CraneHoistSystemsItemDto>> GetCraneHoistSystemsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    /// <summary>Returns the single CraneHoistSystems item matching the identifier, or <see langword="null"/> if not found.</summary>
    Task<CraneHoistSystemsItemDto?> GetCraneHoistSystemsItemByIdAsync(
        Guid tenantId,
        Guid craneHoistSystemsId,
        CancellationToken cancellationToken);

    /// <summary>Returns CraneHoistSystems items whose inspection is due (or overdue) as of <paramref name="asOfUtc"/>.</summary>
    Task<IReadOnlyList<CraneHoistSystemsItemDto>> ListAssetsDueForInspectionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime asOfUtc,
        CancellationToken cancellationToken);

    /// <summary>Returns CraneHoistSystems items whose operator qualification expires within <paramref name="withinDays"/> days.</summary>
    Task<IReadOnlyList<CraneHoistSystemsItemDto>> ListExpiringOperatorQualificationsAsync(
        Guid tenantId,
        Guid? siteId,
        int withinDays,
        CancellationToken cancellationToken);
}
