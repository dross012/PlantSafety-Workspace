// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the SafetySuggestionSubmissions read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Repositories;

/// <summary>Read-only repository for SafetySuggestionSubmissions data. Implemented in the Infrastructure layer.</summary>
public interface ISafetySuggestionSubmissionsRepository
{
    /// <summary>Returns SafetySuggestionSubmissions items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetySuggestionSubmissionsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}