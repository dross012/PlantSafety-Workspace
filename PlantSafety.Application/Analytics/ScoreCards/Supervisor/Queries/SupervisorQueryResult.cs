// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetSupervisorQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Supervisor.Queries;

/// <summary>Result of <see cref="GetSupervisorQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Supervisor items.</param>
public sealed record SupervisorQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SupervisorItemDto> Items);
