// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EmergencySystemsAndLifeSafety read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Repositories;

/// <summary>Read-only repository for EmergencySystemsAndLifeSafety data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencySystemsAndLifeSafetyRepository
{
    /// <summary>Returns EmergencySystemsAndLifeSafety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencySystemsAndLifeSafetyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}