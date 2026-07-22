// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEmergencyResponsePlanningQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Models;

namespace PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Queries;

/// <summary>Result of <see cref="GetEmergencyResponsePlanningQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EmergencyResponsePlanning items.</param>
public sealed record EmergencyResponsePlanningQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencyResponsePlanningItemDto> Items);