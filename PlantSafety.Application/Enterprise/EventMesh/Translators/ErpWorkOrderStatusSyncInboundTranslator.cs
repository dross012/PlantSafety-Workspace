// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.EventMesh
//  Layer:          Application
//  File:           ErpWorkOrderStatusSyncInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Concrete, working inbound translator: an external ERP's work-order-status-sync
//                  event ("erp.workorder.statuschanged.v1") is translated into the platform's real,
//                  public Operational.WorkOrders.Commands.UpdateWorkOrderStatusCommand and dispatched
//                  via ISender -- never by reaching into WorkOrder's Domain aggregate or repository
//                  directly (Handbook Part IV §4.2.2/Part IX). Registered as one of possibly many
//                  IEnterpriseInboundEventTranslator implementations resolved by DI (see
//                  IngestEnterpriseInboundEventCommandHandler); additional ERP/MES/CMMS event types
//                  can be supported by adding sibling translators without modifying this one.
//                  Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Operational.WorkOrders.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.EventMesh.Translators;

/// <summary>Translates an inbound ERP work-order-status-sync event into <see cref="UpdateWorkOrderStatusCommand"/>.</summary>
public sealed class ErpWorkOrderStatusSyncInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "erp.workorder.statuschanged.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Erp &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("workOrderId", out var workOrderIdElement) ||
            !workOrderIdElement.TryGetGuid(out var workOrderId))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
                ErrorType.Validation,
                "ERP work-order-status-sync payload is missing a valid 'workOrderId'."));
        }

        if (!payload.TryGetProperty("targetStatus", out var targetStatusElement) ||
            targetStatusElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(targetStatusElement.GetString()))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
                ErrorType.Validation,
                "ERP work-order-status-sync payload is missing a valid 'targetStatus'."));
        }

        var reason = payload.TryGetProperty("reason", out var reasonElement) && reasonElement.ValueKind == JsonValueKind.String
            ? reasonElement.GetString()
            : "Synchronised from external ERP system.";

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

    /// <summary>
    /// Application.Common.Errors.ApplicationError.ErrorCode uses the platform's own
    /// <c>Module.Entity.Condition</c> (dot-delimited, mixed-case) convention, while
    /// SharedKernel.Primitives.Errors.ErrorCode.Create requires a lowercase, dot-delimited value --
    /// normalising here avoids a thrown ArgumentException when re-wrapping the inner command's error.
    /// </summary>
    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
