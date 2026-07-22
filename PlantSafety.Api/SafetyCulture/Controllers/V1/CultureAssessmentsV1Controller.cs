// ============================================================================
// File: CultureAssessmentsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the SafetyCulture module's CultureAssessment commands and its
//              eight sub-component analytics queries (BehavioralObservationCompletion,
//              LeadershipEngagementFrequency, NearMissReportingRate, PpeComplianceRate,
//              SafetyMeetingParticipation, SafetySuggestionSubmissions, StopWorkActions,
//              TrainingCompletionRate) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/safetyculture/assessments/...
// - This is a NEW SafetyCulture module, distinct from the pre-existing PlantSafety.Api.Analytics.Culture
//   tree; do not merge or relocate.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyCulture.Contracts.Requests;
using PlantSafety.Application.SafetyCulture.Commands;
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.SafetyCulture.Controllers.V1;

/// <summary>V1 controller for Culture Assessment commands and analytics queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safetyculture/assessments")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyCulture")]
public sealed class CultureAssessmentsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CultureAssessmentsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public CultureAssessmentsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new CultureAssessment.</summary>
    /// <response code="200">Culture assessment created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateCultureAssessmentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateCultureAssessmentResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateCultureAssessmentResult>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCultureAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateCultureAssessmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            OwnerId: env.ActorId,
            AssessmentTitle: request.AssessmentTitle,
            PeriodStartUtc: request.PeriodStartUtc,
            PeriodEndUtc: request.PeriodEndUtc,
            Summary: request.Summary);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the score for a CultureAssessment.</summary>
    /// <response code="200">Score recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Culture assessment not found.</response>
    [HttpPost("{cultureAssessmentId:guid}/score")]
    [ProducesResponseType(typeof(ApiResponse<RecordCultureAssessmentScoreResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordCultureAssessmentScoreResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RecordCultureAssessmentScoreResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordScoreAsync(
        Guid cultureAssessmentId,
        [FromBody] RecordCultureAssessmentScoreRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordCultureAssessmentScoreCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            CultureAssessmentId: cultureAssessmentId,
            OverallCultureScore: request.OverallCultureScore,
            LeadershipEngagementScore: request.LeadershipEngagementScore)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes the report for a CultureAssessment.</summary>
    /// <response code="200">Report published successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Culture assessment not found.</response>
    [HttpPost("{cultureAssessmentId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<PublishCultureAssessmentReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PublishCultureAssessmentReportResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PublishCultureAssessmentReportResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishAsync(
        Guid cultureAssessmentId,
        [FromBody] PublishCultureAssessmentReportRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishCultureAssessmentReportCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            CultureAssessmentId: cultureAssessmentId,
            Summary: request.Summary)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Sub-component analytics queries ─────────────────────────────────────

    /// <summary>Returns Behavioral Observation Completion analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/behavioral-observation-completion")]
    [ProducesResponseType(typeof(ApiResponse<BehavioralObservationCompletionQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBehavioralObservationCompletionAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetBehavioralObservationCompletionQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Leadership Engagement Frequency analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/leadership-engagement-frequency")]
    [ProducesResponseType(typeof(ApiResponse<LeadershipEngagementFrequencyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLeadershipEngagementFrequencyAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetLeadershipEngagementFrequencyQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Near Miss Reporting Rate analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/near-miss-reporting-rate")]
    [ProducesResponseType(typeof(ApiResponse<NearMissReportingRateQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNearMissReportingRateAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetNearMissReportingRateQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PPE Compliance Rate analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/ppe-compliance-rate")]
    [ProducesResponseType(typeof(ApiResponse<PpeComplianceRateQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPpeComplianceRateAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPpeComplianceRateQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Safety Meeting Participation analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/safety-meeting-participation")]
    [ProducesResponseType(typeof(ApiResponse<SafetyMeetingParticipationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSafetyMeetingParticipationAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSafetyMeetingParticipationQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Safety Suggestion Submissions analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/safety-suggestion-submissions")]
    [ProducesResponseType(typeof(ApiResponse<SafetySuggestionSubmissionsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSafetySuggestionSubmissionsAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSafetySuggestionSubmissionsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Stop Work Actions analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/stop-work-actions")]
    [ProducesResponseType(typeof(ApiResponse<StopWorkActionsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStopWorkActionsAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetStopWorkActionsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Training Completion Rate analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/training-completion-rate")]
    [ProducesResponseType(typeof(ApiResponse<TrainingCompletionRateQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTrainingCompletionRateAsync(
        [FromQuery] SafetyCultureAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetTrainingCompletionRateQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
