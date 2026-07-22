// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetQualityControlQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Queries;

/// <summary>Result of <see cref="GetQualityControlQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching QualityControl items.</param>
public sealed record QualityControlQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<QualityControlItemDto> Items);