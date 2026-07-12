// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetIndustrySpecificQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Models;

namespace PlantSafety.Application.Compliance.Certification.IndustrySpecific.Queries;

/// <summary>Result of <see cref="GetIndustrySpecificQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IndustrySpecific items.</param>
public sealed record IndustrySpecificQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IndustrySpecificItemDto> Items);
