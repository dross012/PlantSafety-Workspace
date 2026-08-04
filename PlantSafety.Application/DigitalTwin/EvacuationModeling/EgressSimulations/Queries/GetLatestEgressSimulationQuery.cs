// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve the most recent EgressSimulationRun for a zone.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Queries;

/// <summary>Retrieves the most recently started EgressSimulationRun for the given zone.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="ZoneId">Identity of the evacuation zone to filter by.</param>
public sealed record GetLatestEgressSimulationQuery(Guid TenantId, Guid? SiteId, Guid ZoneId) : IRequest<Result<EgressSimulationItemDto?>>;
