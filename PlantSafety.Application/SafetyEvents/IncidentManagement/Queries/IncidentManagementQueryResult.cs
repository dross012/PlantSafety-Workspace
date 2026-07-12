// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetIncidentManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.IncidentManagement.Models;

namespace PlantSafety.Application.SafetyEvents.IncidentManagement.Queries;

/// <summary>Result of <see cref="GetIncidentManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IncidentManagement items.</param>
public sealed record IncidentManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentManagementItemDto> Items);
