// ============================================================================
// File: SafetyEventsBulkV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for SafetyEvents module bulk operations. Supports
//              bulk acknowledgement, bulk escalation, and bulk closure of
//              incidents, near-misses, and hazard reports.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//     PUT api/v1/safety-events/bulk/acknowledge — bulk acknowledge (confirm receipt)
//     PUT api/v1/safety-events/bulk/escalate    — bulk escalate to higher severity
//     PUT api/v1/safety-events/bulk/close       — bulk close resolved events
// - SafetyEvents bulk close requires ClosureReason in Parameters dictionary.
//   The Application layer enforces this; the controller passes Parameters through.
// - Application layer references:
//     PlantSafety.Application.SafetyEvents.Commands.BulkUpdateSafetyEventsCommand
//     PlantSafety.Application.SafetyEvents.Commands.BulkUpdateSafetyEventsResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Models.Bulk;
using Asp.Versioning;

namespace PlantSafety.Api.SafetyEvents.Bulk;

/// <summary>V1 controller for SafetyEvents module bulk operations.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/bulk")]
[RequiresTenant]
public sealed class SafetyEventsBulkV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="SafetyEventsBulkV1Controller"/>.</summary>
    public SafetyEventsBulkV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Bulk-acknowledges multiple safety events, confirming receipt by the responsible party.
    /// </summary>
    /// <param name="request">Resource IDs to acknowledge.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [IdempotencyKey]
    public async Task<IActionResult> BulkAcknowledgeAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
        => await DispatchBulkAsync("Acknowledge", request, ct);

    /// <summary>
    /// Bulk-escalates multiple safety events to a higher severity tier.
    /// Requires <c>EscalationReason</c> in <see cref="BulkOperationRequest.Parameters"/>.
    /// </summary>
    /// <param name="request">Resource IDs to escalate.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("escalate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [IdempotencyKey]
    public async Task<IActionResult> BulkEscalateAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
        => await DispatchBulkAsync("Escalate", request, ct);

    /// <summary>
    /// Bulk-closes multiple safety events.
    /// Requires <c>ClosureReason</c> in <see cref="BulkOperationRequest.Parameters"/>.
    /// </summary>
    /// <param name="request">Resource IDs to close.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [IdempotencyKey]
    public async Task<IActionResult> BulkCloseAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
        => await DispatchBulkAsync("Close", request, ct);

    private async Task<IActionResult> DispatchBulkAsync(
        string                  operation,
        BulkOperationRequest    request,
        CancellationToken       ct)
    {
        var command = new PlantSafety.Application.SafetyEvents.Commands.BulkUpdateSafetyEventsCommand(
            request.TenantId,
            request.SiteId,
            request.ResourceIds,
            operation,
            request.Parameters);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return ApiBadRequest<BulkOperationResponse>(result.Error.Message);

        return ApiOk(new BulkOperationResponse(
            request.ResourceIds.Count,
            result.Value.Succeeded,
            result.Value.Failed,
            result.Value.Results
                  .Select(r => new BulkItemResult(r.Id, r.Success, r.ErrorMessage, r.ErrorCode))
                  .ToList()));
    }
}
