// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEmergencySystemsAndLifeSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Queries;

/// <summary>Result of <see cref="GetEmergencySystemsAndLifeSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EmergencySystemsAndLifeSafety items.</param>
public sealed record EmergencySystemsAndLifeSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencySystemsAndLifeSafetyItemDto> Items);