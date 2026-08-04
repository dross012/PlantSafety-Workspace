// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the MachineSafety V1 controller. Each record carries only the
//   domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId, TenantId,
//   ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate identifiers are
//   bound from the route, not the body.

namespace PlantSafety.Api.MachineSafety.Contracts.Requests;

/// <summary>Shared query-string filter for every MachineSafety sub-component "Get" query.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record MachineSafetyQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── LotoSystems ───────────────────────────────────────────────────────────────
public sealed record RecordAnnualAuditLotoSystemsRequest(bool Compliant, decimal ComplianceScore);
public sealed record VerifyZeroEnergyLotoSystemsRequest(Guid VerifiedByUserId);

// ── MachineGuardsBarriers ────────────────────────────────────────────────────
public sealed record RecordInspectionResultMachineGuardsBarriersRequest(int DeficiencyCount, decimal GuardIntegrityScore);
public sealed record ScheduleInspectionMachineGuardsBarriersRequest(DateTime DueAtUtc);

// ── EmergencyStopDevices ──────────────────────────────────────────────────────
public sealed record RecordTestResultEmergencyStopDevicesRequest(bool Passed, decimal ResponseTimeScore, Guid TestedByUserId);
public sealed record ScheduleTestEmergencyStopDevicesRequest(DateTime DueAtUtc);

// ── PresenceSensingDevices ───────────────────────────────────────────────────
public sealed record RecordCalibrationResultPresenceSensingDevicesRequest(decimal DetectionAccuracyScore, Guid CalibratedByUserId);
public sealed record ScheduleCalibrationPresenceSensingDevicesRequest(DateTime DueAtUtc);

// ── SafetyValvesRelays ───────────────────────────────────────────────────────
public sealed record RecordInspectionResultSafetyValvesRelaysRequest(bool Reliable, decimal ReliabilityScore, Guid InspectedByUserId);
public sealed record ScheduleInspectionSafetyValvesRelaysRequest(DateTime DueAtUtc);

// ── TwoHandControls ──────────────────────────────────────────────────────────
public sealed record RecordTestResultTwoHandControlsRequest(bool Synchronized, decimal SynchronizationScore, Guid TestedByUserId);
public sealed record ScheduleTestTwoHandControlsRequest(DateTime DueAtUtc);

// ── CraneHoistSystems ────────────────────────────────────────────────────────
public sealed record RegisterCraneHoistAssetRequest(Guid? SiteId, string AssetTag, string AssetTitle, string AssetType, decimal RatedLoadCapacityKg);
public sealed record RecordLiftEventRequest(decimal ObservedLoadKg);
public sealed record RecordOverloadEventRequest(decimal ObservedLoadKg);
public sealed record ScheduleCraneInspectionRequest(DateTime DueAtUtc);
public sealed record CompleteCraneInspectionRequest(bool Passed);
public sealed record QualifyOperatorRequest(Guid OperatorUserId, DateTime? ExpiresAtUtc);
public sealed record SuspendOperatorQualificationRequest(string Reason);
public sealed record RecordRiggingInspectionRequest(bool DefectDetected, string? Notes);
public sealed record ListAssetsDueForInspectionRequest(Guid? SiteId, DateTime AsOfUtc);
public sealed record ListExpiringOperatorQualificationsRequest(Guid? SiteId, int WithinDays);
