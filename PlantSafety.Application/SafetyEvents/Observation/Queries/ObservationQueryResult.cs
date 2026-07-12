// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetObservationQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.Observation.Models;

namespace PlantSafety.Application.SafetyEvents.Observation.Queries;

/// <summary>Result of <see cref="GetObservationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Observation items.</param>
public sealed record ObservationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ObservationItemDto> Items);
