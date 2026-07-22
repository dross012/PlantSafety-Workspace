// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the EmployeeSafety V1 controller. Each record carries only the
//   domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId, TenantId,
//   ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate identifiers are
//   bound from the route, not the body.

namespace PlantSafety.Api.EmployeeSafety.Contracts.Requests;

/// <summary>Shared query-string filter for every EmployeeSafety sub-component "Get" query.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record EmployeeSafetyQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── LotoPrograms ─────────────────────────────────────────────────────────────
public sealed record ApplyIsolationLotoProgramsRequest(int LocksAppliedCount);
public sealed record RecordAnnualAuditLotoProgramsRequest(bool Compliant, decimal ComplianceScore, string? Notes);
public sealed record ReleaseLotoProgramsRequest(Guid ReleasedByUserId);
public sealed record VerifyZeroEnergyLotoProgramsRequest(Guid VerifiedByUserId);

// ── EnvironmentalHealthMonitoring ────────────────────────────────────────────
public sealed record BeginCorrectiveActionEnvironmentalHealthMonitoringRequest(string Description);
public sealed record RecordMeasurementEnvironmentalHealthMonitoringRequest(decimal ExposureLevelScore);
public sealed record ResolveExceedanceEnvironmentalHealthMonitoringRequest(decimal ReMeasuredScore);

// ── ErgonomicsAndMaterialHandling ────────────────────────────────────────────
public sealed record ImplementControlsErgonomicsAndMaterialHandlingRequest(string ControlsDescription);
public sealed record RecordAssessmentErgonomicsAndMaterialHandlingRequest(int RiskFactorCount, decimal RiskScore);
public sealed record ScheduleFollowUpErgonomicsAndMaterialHandlingRequest(DateTime DueAtUtc);

// ── HazardCommunicationTraining ──────────────────────────────────────────────
public sealed record DeliverTrainingHazardCommunicationTrainingRequest(int EmployeesTrainedCount);
public sealed record ScheduleNextCycleHazardCommunicationTrainingRequest(DateTime DueAtUtc);
public sealed record VerifyComprehensionHazardCommunicationTrainingRequest(decimal ComprehensionScore);

// ── MachineGuarding ───────────────────────────────────────────────────────────
public sealed record FlagBypassMachineGuardingRequest(string FindingNotes, Guid DetectedByUserId);
public sealed record RecordInspectionResultMachineGuardingRequest(bool Passed, int DeficiencyCount, decimal ConditionScore, Guid InspectedByUserId, string? Notes);
public sealed record RetireMachineGuardingRequest(string Reason);
public sealed record ScheduleCorrectiveActionMachineGuardingRequest(DateTime DueAtUtc);
public sealed record ScheduleInspectionMachineGuardingRequest(DateTime DueAtUtc);
public sealed record VerifyCorrectiveActionMachineGuardingRequest(Guid VerifiedByUserId);

// ── Ppe ───────────────────────────────────────────────────────────────────────
public sealed record FlagForReplacementPpeRequest(string Reason);
public sealed record RecordFitTestPpeRequest(bool Passed);
public sealed record RecordInspectionPpeRequest(decimal ConditionScore);

// ── EmergencyPreparedness ────────────────────────────────────────────────────
public sealed record ConductDrillEmergencyPreparednessRequest(int ParticipantCount);
public sealed record RecordReadinessEmergencyPreparednessRequest(decimal ReadinessScore, string? GapNotes);
public sealed record ScheduleDrillEmergencyPreparednessRequest(DateTime DueAtUtc);
