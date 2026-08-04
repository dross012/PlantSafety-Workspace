// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the EvacuationBottleneck read-model. Implemented in
//                    the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Repositories;

/// <summary>Read-only repository for EvacuationBottleneck data.</summary>
public interface IEvacuationBottleneckRepository
{
    /// <summary>Returns bottleneck history, optionally filtered to a single route, ordered by most recently detected first.</summary>
    Task<IReadOnlyList<EvacuationBottleneckItemDto>> GetHistoryAsync(Guid tenantId, Guid? siteId, Guid? routeId, CancellationToken cancellationToken);
}
