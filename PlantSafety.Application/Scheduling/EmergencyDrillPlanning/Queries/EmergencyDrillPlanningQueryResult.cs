// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEmergencyDrillPlanningQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;

/// <summary>Result of <see cref="GetEmergencyDrillPlanningQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EmergencyDrillPlanning items.</param>
public sealed record EmergencyDrillPlanningQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencyDrillPlanningItemDto> Items);