// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPersonalProtectiveEquipmentQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Models;

namespace PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Queries;

/// <summary>Result of <see cref="GetPersonalProtectiveEquipmentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Personal Protective Equipment (PPE) Policies items.</param>
public sealed record PersonalProtectiveEquipmentQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PersonalProtectiveEquipmentItemDto> Items);