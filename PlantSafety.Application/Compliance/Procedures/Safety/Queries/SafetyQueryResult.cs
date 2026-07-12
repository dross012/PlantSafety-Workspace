// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.Safety.Models;

namespace PlantSafety.Application.Compliance.Procedures.Safety.Queries;

/// <summary>Result of <see cref="GetSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Safety items.</param>
public sealed record SafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SafetyItemDto> Items);
