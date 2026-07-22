// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetInspectingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Queries;

/// <summary>Result of <see cref="GetInspectingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Inspecting items.</param>
public sealed record InspectingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<InspectingItemDto> Items);