// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetPredictiveSafetyQuery.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Queries;

/// <summary>Result of <see cref="GetPredictiveSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PredictiveSafety items.</param>
public sealed record PredictiveSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PredictiveSafetyItemDto> Items);