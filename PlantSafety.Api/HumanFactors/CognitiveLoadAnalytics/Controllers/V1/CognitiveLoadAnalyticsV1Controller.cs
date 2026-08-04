// ============================================================================
// File: CognitiveLoadAnalyticsV1Controller.cs
// Project: PlantSafety.Api
// Module: HumanFactors.CognitiveLoadAnalytics
// Created: 2026-08-04
// Description: V1 controller exposing the Application-layer HumanFactors.CognitiveLoadAnalytics
//              sub-module's Commands and Queries over HTTP: cognitive-load reading capture, task
//              complexity profile definition, workload trend recalculation, and predictive
//              cognitive-risk generation/acknowledgement.
//
// Route note: this controller's route ("api/v{version}/human-factors/cognitive-load-analytics")
// sits alongside (and is distinct from) the generic HumanFactors lifecycle controller at
// "api/v{version}/human-factors".
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// - Data governance: individual cognitive-load data is sensitive personal data. The
//   per-worker GET endpoint additionally requires the HumanFactorsAnalyst policy so
//   cross-worker access stays restricted to authorized roles; cross-worker dashboards should
//   consume aggregated projections only.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.HumanFactors.CognitiveLoadAnalytics.Contracts.Requests;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.HumanFactors.CognitiveLoadAnalytics.Controllers.V1;

/// <summary>V1 controller for the HumanFactors.CognitiveLoadAnalytics sub-module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/human-factors/cognitive-load-analytics")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("HumanFactors.CognitiveLoadAnalytics")]
public sealed class CognitiveLoadAnalyticsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CognitiveLoadAnalyticsV1Controller"/>.</summary>
    public CognitiveLoadAnalyticsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ---- Commands ------------------------------------------------------------

    /// <summary>Records a single worker cognitive-workload measurement.</summary>
    [HttpPost("readings")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordCognitiveLoadReadingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordCognitiveLoadReadingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordCognitiveLoadReadingAsync([FromBody] RecordCognitiveLoadReadingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = RecordCognitiveLoadReadingCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId, shiftType: request.ShiftType, shiftDateUtc: request.ShiftDateUtc,
            workerUserId: request.WorkerUserId, taskComplexityProfileId: request.TaskComplexityProfileId,
            cognitiveLoadScore: request.CognitiveLoadScore, measurementMethod: request.MeasurementMethod,
            measuredAtUtc: request.MeasuredAtUtc, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Defines a task-complexity profile.</summary>
    [HttpPost("task-complexity-profiles")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DefineTaskComplexityProfileResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DefineTaskComplexityProfileResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefineTaskComplexityProfileAsync([FromBody] DefineTaskComplexityProfileRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = DefineTaskComplexityProfileCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId, shiftType: request.ShiftType, shiftDateUtc: request.ShiftDateUtc,
            taskCode: request.TaskCode, taskName: request.TaskName, complexityScore: request.ComplexityScore,
            description: request.Description ?? string.Empty, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Recalculates a ledger's workload trend for a given window (periodic per-shift-cycle scheduling hook).</summary>
    [HttpPost("{cognitiveLoadLedgerId:guid}/workload-trend/recalculate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecalculateWorkloadTrendResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecalculateWorkloadTrendResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecalculateWorkloadTrendAsync(
        Guid cognitiveLoadLedgerId, [FromBody] RecalculateWorkloadTrendRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = RecalculateWorkloadTrendCommand.FromDto(
            tenantId: env.TenantId, cognitiveLoadLedgerId: cognitiveLoadLedgerId,
            periodStartUtc: request.PeriodStartUtc, periodEndUtc: request.PeriodEndUtc, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Generates a predictive cognitive-risk assessment for a worker from a source ledger.</summary>
    [HttpPost("risk-predictions")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<GenerateCognitiveRiskPredictionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GenerateCognitiveRiskPredictionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateCognitiveRiskPredictionAsync([FromBody] GenerateCognitiveRiskPredictionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = GenerateCognitiveRiskPredictionCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId, workerUserId: request.WorkerUserId,
            sourceLedgerId: request.SourceLedgerId, modelVersion: request.ModelVersion, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Acknowledges a generated cognitive-risk alert (backs the acknowledgement-response-time KPI).</summary>
    [HttpPost("risk-predictions/{cognitiveRiskPredictionId:guid}/acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AcknowledgeCognitiveRiskAlertResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AcknowledgeCognitiveRiskAlertResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcknowledgeCognitiveRiskAlertAsync(
        Guid cognitiveRiskPredictionId, [FromBody] AcknowledgeCognitiveRiskAlertRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = AcknowledgeCognitiveRiskAlertCommand.FromDto(
            tenantId: env.TenantId, cognitiveRiskPredictionId: cognitiveRiskPredictionId,
            acknowledgementNotes: request.AcknowledgementNotes, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ---- Queries -------------------------------------------------------------

    /// <summary>
    /// Returns a single worker's cognitive-load rollup for a period. Data governance: restricted
    /// to the worker themselves, their supervisor, or an authorized HumanFactors analyst role.
    /// </summary>
    [HttpGet("workers/{workerUserId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models.CognitiveLoadByWorkerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCognitiveLoadByWorkerAsync(
        Guid workerUserId, [FromQuery] DateTime periodStartUtc, [FromQuery] DateTime periodEndUtc, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCognitiveLoadByWorkerQuery(env.TenantId, workerUserId, periodStartUtc, periodEndUtc);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single task-complexity profile by task code (Task Complexity vs. Load Correlation dashboard).</summary>
    [HttpGet("task-complexity-profiles/{taskCode}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models.TaskComplexityProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskComplexityProfileAsync(string taskCode, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetTaskComplexityProfileQuery(env.TenantId, taskCode);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns workload-trend snapshots for a shift within a period (Cognitive Load by Shift/Role dashboard).</summary>
    [HttpGet("workload-trend")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models.WorkloadTrendByShiftDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkloadTrendByShiftAsync(
        [FromQuery] Guid? siteId, [FromQuery] string shiftType, [FromQuery] DateTime periodStartUtc, [FromQuery] DateTime periodEndUtc,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetWorkloadTrendByShiftQuery(env.TenantId, siteId, shiftType, periodStartUtc, periodEndUtc);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns all Elevated/Critical, non-dismissed cognitive-risk predictions (Predictive Cognitive Risk Board dashboard).</summary>
    [HttpGet("risk-predictions/elevated")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models.CognitiveRiskPredictionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListElevatedCognitiveRiskPredictionsAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListElevatedCognitiveRiskPredictionsQuery(env.TenantId, siteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
