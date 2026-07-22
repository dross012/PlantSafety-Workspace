// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Commands for the real 3-phase Risk Management workflow: Severity x Likelihood
//   scoring, P1-P4 prioritization, Hierarchy-of-Controls measure application, residual-risk
//   effectiveness validation, periodic review, and closure. Carries the full command envelope per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.RiskManagement.Commands;

/// <summary>Records the Severity x Likelihood risk score for a hazard (Phase 1, Step 2).</summary>
public sealed record ScoreRiskCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, int Severity, int Likelihood, Guid ScoredBy) : IRequest<Result<RiskStatusResult>>;

/// <summary>Assigns a P1-P4 remediation priority and documents the finding (Phase 1, Steps 3-4).</summary>
public sealed record PrioritizeRiskCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, string? Priority, Guid PrioritizedBy) : IRequest<Result<RiskStatusResult>>;

/// <summary>Applies a control measure from the Hierarchy of Controls (Phase 2, Steps 5-6).</summary>
public sealed record ApplyRiskControlMeasureCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, string HierarchyLevel, string Description, string? Justification, Guid ResponsiblePersonId)
    : IRequest<Result<ApplyRiskControlMeasureResult>>;

/// <summary>Validates residual risk after controls are applied (Phase 2, Step 7).</summary>
public sealed record ValidateRiskEffectivenessCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, int ResidualSeverity, int ResidualLikelihood, Guid ValidatedBy)
    : IRequest<Result<ValidateRiskEffectivenessResult>>;

/// <summary>Starts the periodic monitoring/review cycle for a Mitigated risk (Phase 3, Steps 9-10).</summary>
public sealed record StartRiskReviewCycleCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, Guid StartedBy) : IRequest<Result<RiskStatusResult>>;

/// <summary>Completes a periodic review cycle, bumping the revision and returning the risk to Mitigated.</summary>
public sealed record CompleteRiskReviewCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, string? NewReviewCadence, Guid ReviewedBy) : IRequest<Result<RiskStatusResult>>;

/// <summary>Formally closes a Mitigated or UnderReview risk.</summary>
public sealed record CloseRiskCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid RiskId, Guid ClosedBy) : IRequest<Result<RiskStatusResult>>;

/// <summary>Result payload shared by Risk lifecycle-transition commands.</summary>
/// <param name="RiskId">Identity of the affected risk.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record RiskStatusResult(Guid RiskId, string Status);

/// <summary>Result of a successful <see cref="ApplyRiskControlMeasureCommand"/> execution.</summary>
/// <param name="RiskId">Identity of the parent risk.</param>
/// <param name="ControlMeasureId">Identity of the newly created control measure.</param>
/// <param name="Status">Lifecycle status of the risk after the transition.</param>
public sealed record ApplyRiskControlMeasureResult(Guid RiskId, Guid ControlMeasureId, string Status);

/// <summary>Result of a successful <see cref="ValidateRiskEffectivenessCommand"/> execution.</summary>
/// <param name="RiskId">Identity of the affected risk.</param>
/// <param name="Status">Lifecycle status after validation.</param>
/// <param name="IsAcceptable">Whether the residual risk was found acceptable (Low/Medium).</param>
/// <param name="ResidualScore">Computed residual Severity x Likelihood score.</param>
public sealed record ValidateRiskEffectivenessResult(Guid RiskId, string Status, bool IsAcceptable, int ResidualScore);
