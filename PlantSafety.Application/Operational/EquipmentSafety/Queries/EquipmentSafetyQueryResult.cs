// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEquipmentSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.EquipmentSafety.Models;

namespace PlantSafety.Application.Operational.EquipmentSafety.Queries;

/// <summary>Result of <see cref="GetEquipmentSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EquipmentSafety items.</param>
public sealed record EquipmentSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EquipmentSafetyItemDto> Items);
