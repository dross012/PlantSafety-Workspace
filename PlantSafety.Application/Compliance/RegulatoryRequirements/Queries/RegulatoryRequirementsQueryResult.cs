// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRegulatoryRequirementsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryRequirements.Models;

namespace PlantSafety.Application.Compliance.RegulatoryRequirements.Queries;

/// <summary>Result of <see cref="GetRegulatoryRequirementsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RegulatoryRequirements items.</param>
public sealed record RegulatoryRequirementsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RegulatoryRequirementsItemDto> Items);
