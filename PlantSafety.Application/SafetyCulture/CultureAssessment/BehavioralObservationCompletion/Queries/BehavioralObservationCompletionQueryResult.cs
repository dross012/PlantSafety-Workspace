// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetBehavioralObservationCompletionQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Queries;

/// <summary>Result of <see cref="GetBehavioralObservationCompletionQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching BehavioralObservationCompletion items.</param>
public sealed record BehavioralObservationCompletionQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<BehavioralObservationCompletionItemDto> Items);