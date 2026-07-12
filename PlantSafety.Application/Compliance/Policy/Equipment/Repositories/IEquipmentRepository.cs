// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Equipment read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Equipment.Models;

namespace PlantSafety.Application.Compliance.Policy.Equipment.Repositories;

/// <summary>Read-only repository for Equipment data. Implemented in the Infrastructure layer.</summary>
public interface IEquipmentRepository
{
    /// <summary>Returns Equipment items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EquipmentItemDto>> GetEquipmentPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
