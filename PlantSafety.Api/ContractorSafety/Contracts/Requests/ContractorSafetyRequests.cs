// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the ContractorSafety V1 controller. Each record carries only the
//   domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId, TenantId,
//   ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate identifiers are
//   bound from the route, not the body.

namespace PlantSafety.Api.ContractorSafety.Contracts.Requests;

/// <summary>Shared query-string filter for every ContractorSafety sub-component "Get" query.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record ContractorSafetyQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── PreQualificationVetting ──────────────────────────────────────────────────
public sealed record ApprovePreQualificationVettingRequest(
    decimal EmrScore, decimal TrirScore, bool OshaTrainingCompliant, int DeficiencyCount,
    DateTime InsuranceExpiresAtUtc, int ValidityPeriodDays);
public sealed record BeginReviewPreQualificationVettingRequest(Guid ReviewedByUserId);
public sealed record RejectPreQualificationVettingRequest(string Reason);

// ── IncidentTrackingPerformanceEvaluation ────────────────────────────────────
public sealed record BeginInvestigationIncidentTrackingPerformanceEvaluationRequest(Guid EvaluatedByUserId);
public sealed record DetermineEligibilityIncidentTrackingPerformanceEvaluationRequest(decimal PerformanceScore);
public sealed record DocumentCapaIncidentTrackingPerformanceEvaluationRequest(int CapaCount, string RootCauseSummary);

// ── PermitToWork ─────────────────────────────────────────────────────────────
public sealed record IssuePermitToWorkRequest(Guid AuthorizedByUserId, int ValidityHours);
public sealed record RecordHazardAssessmentPermitToWorkRequest(int HazardCount, decimal? AtmosphericTestScore);
public sealed record SuspendPermitToWorkRequest(string Reason);

// ── SiteOrientation ───────────────────────────────────────────────────────────
public sealed record DeliverOrientationSiteOrientationRequest(int AttendeeCount);
public sealed record VerifyComprehensionSiteOrientationRequest(decimal ComprehensionScore, int ValidityDays);

// ── ActiveMonitoringAuditing ─────────────────────────────────────────────────
public sealed record RecordObservationActiveMonitoringAuditingRequest(int UnsafeConditionCount, decimal ComplianceScore, string? FindingNotes);
public sealed record ScheduleFollowUpActiveMonitoringAuditingRequest(DateTime DueAtUtc);
