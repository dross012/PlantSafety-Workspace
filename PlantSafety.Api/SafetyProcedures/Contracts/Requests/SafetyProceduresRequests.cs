// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the SafetyProcedures V1 controller. Each record carries only the
//   domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId, TenantId,
//   ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate identifiers are
//   bound from the route, not the body.

namespace PlantSafety.Api.SafetyProcedures.Contracts.Requests;

/// <summary>Shared query-string filter for every SafetyProcedures sub-component "Get" query.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record SafetyProceduresQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── MachineGuardingInspections ───────────────────────────────────────────────
public sealed record CloseMachineGuardingInspectionsRequest(DateTime NextDueAtUtc);
public sealed record ExecuteInspectionMachineGuardingInspectionsRequest(int DeficiencyCount, decimal ComplianceScore, Guid InspectedByUserId);

// ── RoutineSafetyAuditsIncidentReporting ─────────────────────────────────────
public sealed record CloseRoutineSafetyAuditsIncidentReportingRequest(DateTime NextDueAtUtc);
public sealed record ConductRoutineSafetyAuditsIncidentReportingRequest(int FindingCount, decimal ComplianceScore, Guid ConductedByUserId);

// ── MaterialHandlingStorage ──────────────────────────────────────────────────
public sealed record FlagObstructionMaterialHandlingStorageRequest(int ObstructionCount);
public sealed record PublishMaterialHandlingStorageRequest(Guid ApprovedByUserId, int ReviewCycleDays);
public sealed record ResolveObstructionMaterialHandlingStorageRequest(decimal ComplianceScore);

// ── EmergencyResponsePlanning ────────────────────────────────────────────────
public sealed record PublishEmergencyResponsePlanningRequest(Guid ApprovedByUserId);
public sealed record RecordDrillEmergencyResponsePlanningRequest(int ParticipantCount, decimal ReadinessScore);
public sealed record ScheduleDrillEmergencyResponsePlanningRequest(DateTime DueAtUtc);

// ── ErgonomicsWorkstationDesign ──────────────────────────────────────────────
public sealed record PublishErgonomicsWorkstationDesignRequest(Guid ApprovedByUserId);
public sealed record RecordAssessmentErgonomicsWorkstationDesignRequest(int RiskFactorCount, decimal ErgonomicsScore);
public sealed record ScheduleAssessmentErgonomicsWorkstationDesignRequest(DateTime DueAtUtc);

// ── HazardCommunicationRightToKnow ───────────────────────────────────────────
public sealed record PublishHazardCommunicationRightToKnowRequest(Guid ApprovedByUserId, int LabeledItemCount, int ReviewCycleDays);

// ── LotoProcedures ────────────────────────────────────────────────────────────
public sealed record PublishLotoProceduresRequest(Guid ApprovedByUserId, int AuthorizedPersonnelCount);
public sealed record RecordAnnualAuditLotoProceduresRequest(bool Compliant, decimal ComplianceScore);

// ── PpePolicies ───────────────────────────────────────────────────────────────
public sealed record PublishPpePoliciesRequest(Guid ApprovedByUserId, int ReplacementScheduleCount, int ReviewCycleDays);
public sealed record RecordEnforcementAuditPpePoliciesRequest(decimal ComplianceScore);
