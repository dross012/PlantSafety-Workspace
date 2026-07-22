// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetTrainingCompletionRateQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Queries;

/// <summary>Result of <see cref="GetTrainingCompletionRateQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching TrainingCompletionRate items.</param>
public sealed record TrainingCompletionRateQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TrainingCompletionRateItemDto> Items);