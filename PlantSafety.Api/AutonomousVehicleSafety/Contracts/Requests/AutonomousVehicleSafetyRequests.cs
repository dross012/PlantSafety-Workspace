// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the AutonomousVehicleSafety V1 controller. Each record carries
//   only the domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId,
//   TenantId, ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate
//   identifiers are bound from the route, not the body.

namespace PlantSafety.Api.AutonomousVehicleSafety.Contracts.Requests;

/// <summary>Shared query-string filter for every AutonomousVehicleSafety sub-component "list" query.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record AutonomousVehicleSafetyQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── AutonomousVehicleAsset ───────────────────────────────────────────────────
public sealed record RegisterAutonomousVehicleAssetRequest(Guid? SiteId, string AssetTag, string VehicleType, string AutonomyLevel);
public sealed record RecordAutonomyModeTransitionRequest(string ToMode);
public sealed record SuspendAutonomousOperationRequest(string Reason);

// ── OperatingGeofence ─────────────────────────────────────────────────────────
public sealed record DefineOperatingGeofenceRequest(Guid? SiteId, string GeofenceName, string ZoneType, string BoundaryDefinition, decimal? MaxSpeedKph);

// ── SafetyCaseValidation ──────────────────────────────────────────────────────
public sealed record ValidateSafetyCaseRequest(Guid? SiteId, Guid AutonomousVehicleAssetId, string StandardReference, string DocumentVersion, int RevalidationCadenceDays, Guid ValidatedByUserId);

// ── HumanRobotInteractionEvent ────────────────────────────────────────────────
public sealed record RecordHumanRobotInteractionEventRequest(Guid? SiteId, Guid AutonomousVehicleAssetId, string InteractionType, string SeverityLevel, decimal? DistanceMeters, string LocationDescription);

// ── AutonomousVehicleIncident ─────────────────────────────────────────────────
public sealed record RecordAutonomousVehicleIncidentRequest(Guid? SiteId, Guid AutonomousVehicleAssetId, string IncidentType, string SeverityLevel, string DescriptionSummary);
