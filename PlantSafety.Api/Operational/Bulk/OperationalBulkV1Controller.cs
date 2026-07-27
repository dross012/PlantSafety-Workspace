// ============================================================================
// File: OperationalBulkV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for Operational module bulk operations. Supports
//              bulk completion of tasks, bulk permit closure, and bulk LOTO
//              record status updates.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//     PUT api/v1/operational/bulk/complete — bulk complete tasks or work items
//     PUT api/v1/operational/bulk/close    — bulk close permits to work
// - Operational bulk operations are common at shift end when supervisors close
//   out multiple completed work items simultaneously.
// - Application layer references:
//     PlantSafety.Application.Operational.Commands.BulkUpdateOperationalCommand
//     PlantSafety.Application.Operational.Commands.BulkUpdateOperationalResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Models.Bulk;
using Asp.Versioning;

namespace PlantSafety.Api.Operational.Bulk;

/// <summary>V1 controller for Operational module bulk operations.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational/bulk")]
[RequiresTenant]
public sealed class OperationalBulkV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationalBulkV1Controller"/>.</summary>
    public OperationalBulkV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Bulk-completes multiple operational tasks or work items.
    /// </summary>
    /// <param name="request">Resource IDs to mark as complete.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [IdempotencyKey]
    public async Task<IActionResult> BulkCompleteAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
        => await DispatchBulkAsync("Complete", request, ct);

    /// <summary>
    /// Bulk-closes multiple permits to work.
    /// Requires <c>ClosureSignedBy</c> in <see cref="BulkOperationRequest.Parameters"/>.
    /// </summary>
    /// <param name="request">Resource IDs of permits to close.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [IdempotencyKey]
    public async Task<IActionResult> BulkCloseAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
        => await DispatchBulkAsync("Close", request, ct);

    private async Task<IActionResult> DispatchBulkAsync(
        string               operation,
        BulkOperationRequest request,
        CancellationToken    ct)
    {
        var command = new PlantSafety.Application.Operational.Commands.BulkUpdateOperationalCommand(
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
