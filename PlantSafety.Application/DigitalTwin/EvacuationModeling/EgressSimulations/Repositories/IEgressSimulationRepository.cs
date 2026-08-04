// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the EgressSimulationRun read-model. Implemented in
//                    the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Repositories;

/// <summary>Read-only repository for EgressSimulationRun data.</summary>
public interface IEgressSimulationRepository
{
    /// <summary>Returns the most recently started EgressSimulationRun for the given zone, or null if none exist.</summary>
    Task<EgressSimulationItemDto?> GetLatestByZoneAsync(Guid tenantId, Guid? siteId, Guid zoneId, CancellationToken cancellationToken);
}
