// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the AutomatedIncidentDetection read-model.
// SchemaVersion: 1

using PlantSafety.Application.Observability.AutomatedIncidentDetection.Models;

namespace PlantSafety.Application.Observability.AutomatedIncidentDetection.Repositories;

/// <summary>Read-only repository for AutomatedIncidentDetection data. Implemented in the Infrastructure layer.</summary>
public interface IAutomatedIncidentDetectionRepository
{
    /// <summary>Returns AutomatedIncidentDetection items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AutomatedIncidentDetectionItemDto>> GetAutomatedIncidentDetectionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}