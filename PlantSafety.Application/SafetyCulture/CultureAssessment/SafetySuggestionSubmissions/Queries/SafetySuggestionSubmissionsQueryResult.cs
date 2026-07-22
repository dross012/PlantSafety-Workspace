// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetSafetySuggestionSubmissionsQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Queries;

/// <summary>Result of <see cref="GetSafetySuggestionSubmissionsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching SafetySuggestionSubmissions items.</param>
public sealed record SafetySuggestionSubmissionsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SafetySuggestionSubmissionsItemDto> Items);