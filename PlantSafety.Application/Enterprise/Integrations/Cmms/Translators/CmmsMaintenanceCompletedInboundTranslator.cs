// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Cmms
//  Layer:          Application
//  File:           CmmsMaintenanceCompletedInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.3 Enterprise System Integrations, CMMS)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound CMMS maintenance-task-completed event
//                  ("cmms.maintenancetask.completed.v1") into the platform's real, public
//                  Operational.PreventiveMaintenance.Commands.CompletePreventiveMaintenanceTaskCommand
//                  -- closing the loop on asset maintenance history sync (Phase 6 §1.3). Additive-
//                  only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Operational.PreventiveMaintenance.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Cmms.Translators;

/// <summary>Translates an inbound CMMS maintenance-task-completed event into <see cref="CompletePreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class CmmsMaintenanceCompletedInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "cmms.maintenancetask.completed.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Cmms &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("preventiveMaintenanceTaskId", out var taskIdElement) || !taskIdElement.TryGetGuid(out var taskId))
            return InvalidPayload("'preventiveMaintenanceTaskId'");

        var completionNotes = payload.TryGetProperty("completionNotes", out var notesElement) && notesElement.ValueKind == JsonValueKind.String
            ? notesElement.GetString()
            : "Completed via external CMMS system.";

        var command = new CompletePreventiveMaintenanceTaskCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            TenantId: envelope.TenantId,
            PreventiveMaintenanceTaskId: taskId,
            CompletedByUserId: envelope.ActorId ?? Guid.Empty,
            CompletionNotes: completionNotes);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(result.Error.ErrorCode)),
                ErrorType.Failure,
                result.Error.Message));
    }

    private static Result<bool> InvalidPayload(string missingField) =>
        Result<bool>.Failure(new Error(
            ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
            ErrorType.Validation,
            $"CMMS maintenance-task-completed payload is missing a valid {missingField}."));

    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
