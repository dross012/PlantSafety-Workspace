// ============================================================================
// File: ComplianceBulkV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for Compliance module bulk operations. Supports
//              bulk status update, bulk approval, and bulk reassignment across
//              any Compliance sub-resource (AuditFindings, CorrectiveActions, etc.).
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//     PUT api/v1/compliance/bulk/status  — bulk status change (Close, Reopen, etc.)
//     PUT api/v1/compliance/bulk/approve — bulk approval workflow transition
//     PUT api/v1/compliance/bulk/assign  — bulk assignee change
// - Max 500 resource IDs per request (enforced by [MaxLength(500)] on BulkOperationRequest).
// - Partial-success semantics: always returns 200 with BulkOperationResponse.
//   Inspect HasPartialFailure and Results to identify failed items.
// - [IdempotencyKey] is applied to all bulk actions because they are POST/PUT
//   operations that modify state and may be retried by clients on timeout.
// - Application layer references:
//     PlantSafety.Application.Compliance.Commands.BulkUpdateComplianceCommand
//     PlantSafety.Application.Compliance.Commands.BulkUpdateComplianceResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Models.Bulk;
using Asp.Versioning;

namespace PlantSafety.Api.Compliance.Bulk;

/// <summary>V1 controller for Compliance module bulk operations.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/bulk")]
[RequiresTenant]
public sealed class ComplianceBulkV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="ComplianceBulkV1Controller"/>.</summary>
    public ComplianceBulkV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Updates the status of multiple Compliance resources in a single operation.
    /// Valid operations: <c>"Close"</c>, <c>"Reopen"</c>, <c>"Cancel"</c>.
    /// </summary>
    /// <param name="request">Bulk operation request with resource IDs and target status.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("status")]
    [Authorize(Policy = "Compliance.Update")]
    [IdempotencyKey]
    public async Task<IActionResult> BulkUpdateStatusAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
    {
        var command = new PlantSafety.Application.Compliance.Commands.BulkUpdateComplianceCommand(
            request.TenantId,
            request.SiteId,
            request.ResourceIds,
            request.Operation,
            request.Parameters);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return ApiBadRequest<BulkOperationResponse>(result.Error.Message);

        return ApiOk(MapToResponse(request.ResourceIds.Count, result.Value));
    }

    /// <summary>
    /// Bulk-approves multiple Compliance resources, transitioning them through the
    /// approval workflow. Requires <c>Compliance.Approve</c> policy.
    /// </summary>
    /// <param name="request">Bulk operation request. <c>Operation</c> should be <c>"Approve"</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("approve")]
    [Authorize(Policy = "Compliance.Approve")]
    [IdempotencyKey]
    public async Task<IActionResult> BulkApproveAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
    {
        var command = new PlantSafety.Application.Compliance.Commands.BulkUpdateComplianceCommand(
            request.TenantId,
            request.SiteId,
            request.ResourceIds,
            "Approve",
            request.Parameters);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return ApiBadRequest<BulkOperationResponse>(result.Error.Message);

        return ApiOk(MapToResponse(request.ResourceIds.Count, result.Value));
    }

    /// <summary>
    /// Reassigns multiple Compliance resources to a different user.
    /// Requires <c>AssignedToUserId</c> in <see cref="BulkOperationRequest.Parameters"/>.
    /// </summary>
    /// <param name="request">Bulk operation request. Must include <c>Parameters["AssignedToUserId"]</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("assign")]
    [Authorize(Policy = "Compliance.Update")]
    [IdempotencyKey]
    public async Task<IActionResult> BulkAssignAsync(
        [FromBody] BulkOperationRequest request,
        CancellationToken               ct)
    {
        var command = new PlantSafety.Application.Compliance.Commands.BulkUpdateComplianceCommand(
            request.TenantId,
            request.SiteId,
            request.ResourceIds,
            "Reassign",
            request.Parameters);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return ApiBadRequest<BulkOperationResponse>(result.Error.Message);

        return ApiOk(MapToResponse(request.ResourceIds.Count, result.Value));
    }

    private static BulkOperationResponse MapToResponse(
        int totalRequested,
        PlantSafety.Application.Compliance.Commands.BulkUpdateComplianceResult value) =>
        new(
            totalRequested,
            value.Succeeded,
            value.Failed,
            value.Results.Select(r => new BulkItemResult(r.Id, r.Success, r.ErrorMessage, r.ErrorCode))
                         .ToList());
}
