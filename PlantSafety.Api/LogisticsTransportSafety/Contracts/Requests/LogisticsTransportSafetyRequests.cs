// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the LogisticsTransportSafety V1 controller. Each record carries only
//   the domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId, TenantId,
//   ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate identifiers are
//   bound from the route, not the body.

namespace PlantSafety.Api.LogisticsTransportSafety.Contracts.Requests;

/// <summary>Shared query-string filter for LogisticsTransportSafety sub-component "Get" queries with a date range.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record LogisticsTransportSafetyQueryRequest(Guid? SiteId, DateTime From, DateTime To);

/// <summary>Shared query-string filter for LogisticsTransportSafety sub-component "list active" queries.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record LogisticsTransportSafetyActiveRequest(Guid? SiteId);

// ── TransportVehicles ─────────────────────────────────────────────────────────
public sealed record RegisterTransportVehicleRequest(Guid? SiteId, string VehicleTitle, string VehicleType, string VehicleIdentifier);
public sealed record ScheduleInspectionTransportVehicleRequest(DateTime DueAtUtc);
public sealed record RecordInspectionResultTransportVehicleRequest(bool Passed, decimal InspectionScore);

// ── Carriers ──────────────────────────────────────────────────────────────────
public sealed record RegisterCarrierRequest(Guid? SiteId, string CarrierName, string CarrierType, DateTime? InsuranceExpiryAtUtc);
public sealed record QualifyCarrierRequest(decimal SafetyRatingScore);
public sealed record SuspendCarrierRequest(string Reason);

// ── DriverQualifications ─────────────────────────────────────────────────────
public sealed record QualifyDriverRequest(Guid? SiteId, Guid DriverUserId, string QualificationType, string LicenseNumber, DateTime IssuedAtUtc, DateTime ExpiresAtUtc);
public sealed record SuspendDriverQualificationRequest(string Reason);

// ── TransportTrips ────────────────────────────────────────────────────────────
public sealed record PlanTransportTripRequest(Guid? SiteId, Guid VehicleId, Guid DriverUserId, string TripType, string RouteDescription, DateTime PlannedDepartureAtUtc, DateTime PlannedArrivalAtUtc);
public sealed record CompleteTransportTripRequest(DateTime ActualDepartureAtUtc, DateTime ActualArrivalAtUtc, decimal DistanceMiles);

// ── LoadSecurementChecks ─────────────────────────────────────────────────────
public sealed record RecordLoadSecurementCheckRequest(Guid? SiteId, Guid? TripId, Guid VehicleId, Guid CheckedByUserId, bool Passed, int DeficiencyCount, decimal SecurementScore);

// ── TransportIncidents ────────────────────────────────────────────────────────
public sealed record RecordTransportIncidentRequest(Guid? SiteId, Guid? TripId, Guid VehicleId, Guid DriverUserId, string IncidentType, string Severity, string Location);
