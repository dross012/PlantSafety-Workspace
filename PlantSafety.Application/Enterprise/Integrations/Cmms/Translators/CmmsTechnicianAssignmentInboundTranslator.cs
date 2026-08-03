// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Cmms
//  Layer:          Application
//  File:           CmmsTechnicianAssignmentInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.3 Enterprise System Integrations, CMMS)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound CMMS technician-assignment event
//                  ("cmms.workorder.technicianassigned.v1") into the platform's real, public
//                  Operational.WorkOrders.Commands.AssignWorkOrderCommand. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Operational.WorkOrders.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Cmms.Translators;

/// <summary>Translates an inbound CMMS technician-assignment event into <see cref="AssignWorkOrderCommand"/>.</summary>
public sealed class CmmsTechnicianAssignmentInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "cmms.workorder.technicianassigned.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Cmms &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("workOrderId", out var workOrderIdElement) || !workOrderIdElement.TryGetGuid(out var workOrderId))
            return InvalidPayload("'workOrderId'");

        if (!payload.TryGetProperty("technicianId", out var technicianIdElement) || !technicianIdElement.TryGetGuid(out var technicianId))
            return InvalidPayload("'technicianId'");

        var technicianName = payload.TryGetProperty("technicianName", out var technicianNameElement) &&
            technicianNameElement.ValueKind == JsonValueKind.String
            ? technicianNameElement.GetString() ?? string.Empty
            : string.Empty;

        var command = new AssignWorkOrderCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            TenantId: envelope.TenantId,
            WorkOrderId: workOrderId,
            TechnicianId: technicianId,
            TechnicianName: technicianName,
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
            $"CMMS technician-assignment payload is missing a valid {missingField}."));

    /// <inheritdoc cref="CmmsWorkOrderStatusSyncInboundTranslator" path="//remarks"/>
    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
