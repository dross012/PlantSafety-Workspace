// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListPublicSafetyAgenciesQuery.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;

/// <summary>Result of <see cref="ListPublicSafetyAgenciesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PublicSafetyAgency items.</param>
public sealed record ListPublicSafetyAgenciesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PublicSafetyAgencyItemDto> Items);
