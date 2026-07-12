// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the EquipmentSafety read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.EquipmentSafety.Models;

namespace PlantSafety.Application.Operational.EquipmentSafety.Repositories;

/// <summary>Read-only repository for EquipmentSafety data. Implemented in the Infrastructure layer.</summary>
public interface IEquipmentSafetyRepository
{
    /// <summary>Returns EquipmentSafety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EquipmentSafetyItemDto>> GetEquipmentSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
