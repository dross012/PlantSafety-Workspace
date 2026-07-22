// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Commands for the real Scorecard compilation/distribution workflow: KPI
//   measurement recording (per the 14-metric catalog), compilation completion, publication,
//   distribution-channel recording, and period closure. Carries the full command envelope per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scorecards.Commands;

/// <summary>Records (or overwrites) a KPI measurement for the scorecard's reporting period.</summary>
public sealed record RecordScorecardMetricCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid ScorecardId, string Metric, decimal Numerator, decimal Denominator, string DataSourceModule, Guid RecordedBy)
    : IRequest<Result<RecordScorecardMetricResult>>;

/// <summary>Marks KPI compilation complete for the reporting period.</summary>
public sealed record CompleteScorecardCompilationCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid ScorecardId, Guid CompletedBy) : IRequest<Result<ScorecardStatusResult>>;

/// <summary>Publishes a Compiled scorecard.</summary>
public sealed record PublishScorecardCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid ScorecardId, Guid PublishedBy) : IRequest<Result<ScorecardStatusResult>>;

/// <summary>Records a distribution event for a Published scorecard (dashboard, leadership review, OSHA inspection, posted area, meeting, annual report).</summary>
public sealed record RecordScorecardDistributionCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid ScorecardId, string Channel, Guid DistributedBy) : IRequest<Result<ScorecardStatusResult>>;

/// <summary>Closes the scorecard once its reporting period has ended.</summary>
public sealed record CloseScorecardCommand(
    Guid CommandId, Guid CorrelationId, Guid CausationId, DateTime RequestedAtUtc,
    Guid TenantId, Guid ScorecardId, Guid ClosedBy) : IRequest<Result<ScorecardStatusResult>>;

/// <summary>Result payload shared by Scorecard lifecycle-transition commands.</summary>
/// <param name="ScorecardId">Identity of the affected scorecard.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ScorecardStatusResult(Guid ScorecardId, string Status);

/// <summary>Result of a successful <see cref="RecordScorecardMetricCommand"/> execution.</summary>
/// <param name="ScorecardId">Identity of the parent scorecard.</param>
/// <param name="KpiMeasurementId">Identity of the recorded measurement.</param>
/// <param name="ComputedValue">The metric value computed from the supplied numerator/denominator.</param>
/// <param name="Status">Lifecycle status of the scorecard after the transition.</param>
public sealed record RecordScorecardMetricResult(Guid ScorecardId, Guid KpiMeasurementId, decimal ComputedValue, string Status);
