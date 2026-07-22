// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Personal Protective Equipment (PPE) Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Models;

namespace PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Repositories;

/// <summary>Read-only repository for Personal Protective Equipment (PPE) Policies data. Implemented in the Infrastructure layer.</summary>
public interface IPersonalProtectiveEquipmentRepository
{
    /// <summary>Returns Personal Protective Equipment (PPE) Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PersonalProtectiveEquipmentItemDto>> GetPersonalProtectiveEquipmentAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}