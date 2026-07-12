// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetTrainingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Training.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Training.Queries;

/// <summary>Result of <see cref="GetTrainingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Training items.</param>
public sealed record TrainingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TrainingItemDto> Items);
