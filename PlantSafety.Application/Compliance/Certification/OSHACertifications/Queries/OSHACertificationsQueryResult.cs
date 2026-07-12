// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetOSHACertificationsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.OSHACertifications.Models;

namespace PlantSafety.Application.Compliance.Certification.OSHACertifications.Queries;

/// <summary>Result of <see cref="GetOSHACertificationsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching OSHACertifications items.</param>
public sealed record OSHACertificationsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<OSHACertificationsItemDto> Items);
