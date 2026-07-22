// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTOs for the Scorecards module's commands. Domain fields only;
//   envelope fields (CommandId/CorrelationId/TenantId/ActorId/etc.) are populated server-side
//   by RequestEnvelopeFactory.
// SchemaVersion: 1

namespace PlantSafety.Api.Scorecards.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/scorecards</c>.</summary>
public sealed class CreateScorecardRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable title for the new scorecard record.</summary>
    public required string ScorecardTitle { get; init; }

    /// <summary>IndicatorCategory code governing this record's primary panel emphasis.</summary>
    public required string Category { get; init; }

    /// <summary>ScorecardPeriodType code (Monthly/Quarterly/Annual).</summary>
    public required string PeriodType { get; init; }

    /// <summary>UTC instant the reporting period starts.</summary>
    public required DateTime PeriodStartUtc { get; init; }

    /// <summary>Identity of the accountable owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Whether formal review/acknowledgment is required.</summary>
    public bool ReviewRequired { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/scorecards/{scorecardId}/metrics</c>.</summary>
public sealed class RecordScorecardMetricRequest
{
    /// <summary>KPI metric code from the 14-metric catalog.</summary>
    public required string Metric { get; init; }

    /// <summary>Numerator for the metric computation.</summary>
    public decimal Numerator { get; init; }

    /// <summary>Denominator for the metric computation.</summary>
    public decimal Denominator { get; init; }

    /// <summary>Source module the underlying counts were sourced from.</summary>
    public required string DataSourceModule { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/scorecards/{scorecardId}/distribute</c>.</summary>
public sealed class RecordScorecardDistributionRequest
{
    /// <summary>Distribution channel (dashboard, leadership review, OSHA inspection, posted area, meeting, annual report).</summary>
    public required string Channel { get; init; }
}

/// <summary>Query-string filters shared by all Scorecards category read-model endpoints.</summary>
public sealed class CategoryQueryRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the UTC date range.</summary>
    public DateTime From { get; init; }

    /// <summary>Inclusive end of the UTC date range.</summary>
    public DateTime To { get; init; }
}
