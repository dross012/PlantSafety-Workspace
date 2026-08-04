// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the EnvironmentalHealth.WaterDischargeCompliance V1 controller.
//   Each record carries only the domain-specific fields for one command/query; envelope fields
//   (CommandId, CorrelationId, TenantId, ActorId, etc.) are populated server-side via
//   RequestEnvelopeFactory, and aggregate identifiers are bound from the route, not the body.

namespace PlantSafety.Api.EnvironmentalHealth.Contracts.Requests;

/// <summary>Shared query-string filter for tenant/site-scoped WaterDischargeCompliance "Get" queries.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record WaterDischargeComplianceSiteQueryRequest(Guid? SiteId);

/// <summary>Query-string filter for date-range-scoped WaterDischargeCompliance "Get" queries.</summary>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record WaterDischargeComplianceDateRangeQueryRequest(DateTime From, DateTime To);

// -- DischargePermit ---------------------------------------------------------
public sealed record IssueDischargePermitRequest(
    string PermitNumber, string RegulatoryProgram, string IssuingAuthority,
    DateTime EffectiveAtUtc, DateTime ExpiresAtUtc, string PermitConditionSummary);
public sealed record RenewDischargePermitRequest(DateTime NewExpiresAtUtc);
public sealed record SuspendDischargePermitRequest(string Reason);
public sealed record RevokeDischargePermitRequest(string Reason);

// -- DischargePoint -----------------------------------------------------------
public sealed record RegisterDischargePointRequest(
    Guid DischargePermitId, string PointCode, string PointName, string ReceivingWaterBody,
    string Outfall, decimal? Latitude, decimal? Longitude);
public sealed record DeactivateDischargePointRequest(string? Reason);
public sealed record DecommissionDischargePointRequest(string? Reason);

// -- EffluentSample -------------------------------------------------------------
public sealed record RecordEffluentSampleRequest(
    Guid DischargePointId, string Parameter, decimal MeasuredValue, string UnitOfMeasure,
    DateTime SampledAtUtc, Guid CollectedByUserId, string LabSourceSystem, string ChainOfCustodyReference,
    Guid? CorrectionOfSampleId, string? CorrectionReason);
public sealed record VoidEffluentSampleRequest(string Reason);
public sealed record SampleHistoryQueryRequest(DateTime From, DateTime To);

// -- DischargeLimit -------------------------------------------------------------
public sealed record DefineDischargeLimitRequest(
    Guid DischargePointId, Guid DischargePermitId, string Parameter, string LimitBasis, decimal LimitValue,
    string UnitOfMeasure, DateTime EffectiveAtUtc, string PermitConditionReference);
public sealed record ReviseDischargeLimitRequest(decimal NewLimitValue);
public sealed record RetireDischargeLimitRequest(string? Reason);

// -- LimitExceedance -------------------------------------------------------------
public sealed record RecordLimitExceedanceRequest(
    Guid DischargePointId, Guid EffluentSampleId, Guid DischargeLimitId, string ParameterName,
    decimal MeasuredValue, decimal LimitValue, string Severity, DateTime DetectedAtUtc, int RepeatExceedanceCount);
public sealed record InitiateExceedanceResponseRequest(Guid ResponseUserId, string? Notes);
public sealed record CloseExceedanceResponseRequest(Guid ResponseUserId, string? ResolutionNotes);

// -- DischargeMonitoringReport ----------------------------------------------------
public sealed record PrepareDischargeMonitoringReportRequest(
    Guid DischargePermitId, DateTime ReportingPeriodStartUtc, DateTime ReportingPeriodEndUtc, DateTime DueAtUtc);
public sealed record SubmitDischargeMonitoringReportRequest(DateTime SubmittedAtUtc, int SamplesWithinLimitCount, int TotalSamplesCount);
public sealed record RejectDischargeMonitoringReportRequest(string Reason);
public sealed record DmrPeriodQueryRequest(Guid? SiteId, DateTime PeriodStartUtc, DateTime PeriodEndUtc);