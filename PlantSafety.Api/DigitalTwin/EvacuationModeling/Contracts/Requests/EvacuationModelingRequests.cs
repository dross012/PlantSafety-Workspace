// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the DigitalTwin.EvacuationModeling V1 controller. Each record
//   carries only the domain-specific fields for one command/query; envelope fields (CommandId,
//   CorrelationId, TenantId, ActorId, etc.) are populated server-side via RequestEnvelopeFactory,
//   and aggregate identifiers are bound from the route, not the body.
// SchemaVersion: 1

namespace PlantSafety.Api.DigitalTwin.EvacuationModeling.Contracts.Requests;

/// <summary>Shared query-string filter for date-ranged EvacuationModeling "Get" queries.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record EvacuationModelingQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── EvacuationRoutes ──────────────────────────────────────────────────────────
public sealed record DefineEvacuationRouteRequest(
    Guid? SiteId, string RouteName, Guid ZoneId, Guid AssemblyPointId,
    string PathGeometryRef, string TwinModelVersion, int EstimatedEgressTimeSeconds);

public sealed record RecordRouteObstructionRequest(string Description, string Severity);

public sealed record ClearRouteObstructionRequest(Guid RouteObstructionId);

// ── EvacuationZones ───────────────────────────────────────────────────────────
public sealed record DefineEvacuationZoneRequest(
    Guid? SiteId, string ZoneName, string TwinModelVersion, int ZoneCapacity, int OccupancyThreshold);

public sealed record RecordOccupancySnapshotRequest(int HeadcountEstimate, string Source);

// ── EgressSimulations ─────────────────────────────────────────────────────────
public sealed record RunEgressSimulationRequest(
    Guid? SiteId, Guid ZoneId, Guid? RouteId, string TwinModelVersion, string TriggerReason,
    int SimulatedEvacuationTimeSeconds, int? PredictedPeakOccupancy);

public sealed record LinkSimulationToDrillRequest(Guid DrillId);
