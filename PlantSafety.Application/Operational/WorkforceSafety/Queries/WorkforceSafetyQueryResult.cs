// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Result payload for GetWorkforceSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.WorkforceSafety.Models;

namespace PlantSafety.Application.Operational.WorkforceSafety.Queries;

/// <summary>Result of <see cref="GetWorkforceSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching WorkforceSafety items.</param>
public sealed record WorkforceSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WorkforceSafetyItemDto> Items);