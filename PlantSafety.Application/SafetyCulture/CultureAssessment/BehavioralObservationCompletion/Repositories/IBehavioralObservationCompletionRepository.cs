// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the BehavioralObservationCompletion read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Repositories;

/// <summary>Read-only repository for BehavioralObservationCompletion data. Implemented in the Infrastructure layer.</summary>
public interface IBehavioralObservationCompletionRepository
{
    /// <summary>Returns BehavioralObservationCompletion items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<BehavioralObservationCompletionItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}