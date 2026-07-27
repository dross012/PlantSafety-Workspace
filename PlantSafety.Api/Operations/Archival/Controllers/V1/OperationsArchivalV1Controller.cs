// ============================================================================
// File: OperationsArchivalV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller surfacing the real Phase 7 Governance retention/purge
//   automation (74-retention-archival.sql: sp_AuditLog_Purge, sp_ErrorLog_Purge,
//   sp_IntegrationLogEntry_Purge, sp_ReplayRecord_Purge, sp_DiagnosticEvent_Purge) and
//   Phase 18 (Invoke-PlantSafetyPurge.ps1 Task Scheduler automation). Routes all requests
//   through MediatR ISender to the Application layer. No domain logic or data access here.
//
//   SAFETY NOTE: "trigger" runs the SAME procedures that already execute unattended daily
//   via Windows Task Scheduler -- it only ever deletes rows older than a TierPolicy-governed
//   retention cutoff, never all rows, and sp_ErrorLog_Purge is additionally hard-blocked at
//   the database level by TR_ErrorLog_NoDelete regardless of caller. See
//   ArchivalDapperRepository for the full trigger-strategy rationale.
// ============================================================================
using PlantSafety.Api.Operations.Archival.Contracts.Requests;
using PlantSafety.Application.Operations.Archival.Commands;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Queries;

namespace PlantSafety.Api.Operations.Archival.Controllers.V1;

/// <summary>V1 controller for the Operations.Archival module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/archival")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.Archival")]
public sealed class OperationsArchivalV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsArchivalV1Controller"/>.</summary>
    public OperationsArchivalV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>Returns static metadata describing the 5 live purge targets.</summary>
    [HttpGet("purge/targets")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurgeTargetDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurgeTargetsAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPurgeTargetsQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<PurgeTargetDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<PurgeTargetDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Triggers an on-demand retention purge for a single target, or all 5 targets when
    /// no target is supplied. Synchronous -- returns the REAL Completed/Blocked/Failed
    /// outcome each procedure logged, not a "Started" acknowledgement.
    /// </summary>
    [HttpPost("purge/trigger")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurgeTriggerResultDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurgeTriggerResultDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TriggerPurgeAsync([FromBody] TriggerPurgeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new TriggerPurgeCommand(request.Target), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<PurgeTriggerResultDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<PurgeTriggerResultDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Returns recent purge run history from Archival.ArchivalJob, newest first.</summary>
    [HttpGet("purge/status")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ArchivalJobDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurgeStatusAsync(
        [FromQuery] string? zone,
        [FromQuery] int top = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetPurgeStatusQuery(zone, top), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<ArchivalJobDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<ArchivalJobDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Returns the current retention windows from Archival.TierPolicy (read-only).</summary>
    [HttpGet("retention-settings")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TierPolicyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRetentionSettingsAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRetentionSettingsQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<TierPolicyDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<TierPolicyDto>>.Ok(result.Value, CorrelationId));
    }
}
