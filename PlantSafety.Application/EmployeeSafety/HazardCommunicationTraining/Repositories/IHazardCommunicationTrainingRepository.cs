// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the HazardCommunicationTraining read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Models;

namespace PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Repositories;

/// <summary>Read-only repository for HazardCommunicationTraining data. Implemented in the Infrastructure layer.</summary>
public interface IHazardCommunicationTrainingRepository
{
    /// <summary>Returns HazardCommunicationTraining items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardCommunicationTrainingItemDto>> GetHazardCommunicationTrainingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}