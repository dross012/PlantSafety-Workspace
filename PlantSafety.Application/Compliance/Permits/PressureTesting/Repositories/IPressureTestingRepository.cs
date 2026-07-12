// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PressureTesting read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.PressureTesting.Models;

namespace PlantSafety.Application.Compliance.Permits.PressureTesting.Repositories;

/// <summary>Read-only repository for PressureTesting data. Implemented in the Infrastructure layer.</summary>
public interface IPressureTestingRepository
{
    /// <summary>Returns PressureTesting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PressureTestingItemDto>> GetPressureTestingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
