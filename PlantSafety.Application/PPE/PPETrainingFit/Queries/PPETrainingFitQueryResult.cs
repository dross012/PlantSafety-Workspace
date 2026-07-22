// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPPETrainingFitQuery.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPETrainingFit.Models;

namespace PlantSafety.Application.PPE.PPETrainingFit.Queries;

/// <summary>Result of <see cref="GetPPETrainingFitQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PPETrainingFit items.</param>
public sealed record PPETrainingFitQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PPETrainingFitItemDto> Items);