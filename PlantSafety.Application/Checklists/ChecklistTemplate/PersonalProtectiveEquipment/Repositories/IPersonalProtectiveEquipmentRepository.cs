// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PersonalProtectiveEquipment read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Repositories;

/// <summary>Read-only repository for PersonalProtectiveEquipment data. Implemented in the Infrastructure layer.</summary>
public interface IPersonalProtectiveEquipmentRepository
{
    /// <summary>Returns PersonalProtectiveEquipment items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PersonalProtectiveEquipmentItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}