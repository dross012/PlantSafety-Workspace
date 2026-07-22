// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the HazardCommunicationRightToKnow read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Models;

namespace PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Repositories;

/// <summary>Read-only repository for HazardCommunicationRightToKnow data. Implemented in the Infrastructure layer.</summary>
public interface IHazardCommunicationRightToKnowRepository
{
    /// <summary>Returns HazardCommunicationRightToKnow items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardCommunicationRightToKnowItemDto>> GetHazardCommunicationRightToKnowItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}