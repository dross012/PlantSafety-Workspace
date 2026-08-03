// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Cmms
//  Layer:          Application
//  File:           CmmsWorkOrderStatusSyncInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.3 Enterprise System Integrations, CMMS)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound CMMS work-order-status-sync event
//                  ("cmms.workorder.statuschanged.v1") into the platform's real, public
//                  Operational.WorkOrders.Commands.UpdateWorkOrderStatusCommand -- CMMS integration
//                  is conceptually the same integration shape as the existing ERP work-order-sync
//                  pattern (see ErpWorkOrderStatusSyncInboundTranslator, which this file mirrors
//                  exactly), just a different external system name/endpoint. Additive-only
//                  evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Operational.WorkOrders.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Cmms.Translators;

/// <summary>Translates an inbound CMMS work-order-status-sync event into <see cref="UpdateWorkOrderStatusCommand"/>.</summary>
public sealed class CmmsWorkOrderStatusSyncInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "cmms.workorder.statuschanged.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Cmms &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("workOrderId", out var workOrderIdElement) ||
            !workOrderIdElement.TryGetGuid(out var workOrderId))
        {
            return InvalidPayload("'workOrderId'");
        }

        if (!payload.TryGetProperty("targetStatus", out var targetStatusElement) ||
            targetStatusElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(targetStatusElement.GetString()))
        {
            return InvalidPayload("'targetStatus'");
        }

        var reason = payload.TryGetProperty("reason", out var reasonElement) && reasonElement.ValueKind == JsonValueKind.String
            ? reasonElement.GetString()
            : "Synchronised from external CMMS system.";

        var command = new UpdateWorkOrderStatusCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            TenantId: envelope.TenantId,
            WorkOrderId: workOrderId,
            TargetStatus: targetStatusElement.GetString()!,
            Reason: reason,
            RequestedByUserId: envelope.ActorId ?? Guid.Empty);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(result.Error!.ErrorCode)),
                ErrorType.Failure,
                result.Error.Message));
    }

    private static Result<bool> InvalidPayload(string missingField) =>
        Result<bool>.Failure(new Error(
            ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
            ErrorType.Validation,
            $"CMMS work-order-status-sync payload is missing a valid {missingField}."));

    /// <summary>
    /// Application.Common.Errors.ApplicationError.ErrorCode uses the platform's own
    /// <c>Module.Entity.Condition</c> (dot-delimited, mixed-case) convention, while
    /// SharedKernel.Primitives.Errors.ErrorCode.Create requires a lowercase, dot-delimited value --
    /// normalising here avoids a thrown ArgumentException when re-wrapping the inner command's error.
    /// </summary>
    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
