// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Cmms
//  Layer:          Application
//  File:           CmmsMaintenancePlanScheduleInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.3 Enterprise System Integrations, CMMS)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound CMMS maintenance-plan-scheduled event
//                  ("cmms.maintenanceplan.scheduled.v1") into the platform's real, public
//                  Operational.PreventiveMaintenance.Commands.SchedulePreventiveMaintenanceTaskCommand.
//                  Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Operational.PreventiveMaintenance.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Cmms.Translators;

/// <summary>Translates an inbound CMMS maintenance-plan-scheduled event into <see cref="SchedulePreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class CmmsMaintenancePlanScheduleInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "cmms.maintenanceplan.scheduled.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Cmms &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("equipmentId", out var equipmentIdElement) || !equipmentIdElement.TryGetGuid(out var equipmentId))
            return InvalidPayload("'equipmentId'");

        var taskDescription = payload.TryGetProperty("taskDescription", out var descriptionElement) &&
            descriptionElement.ValueKind == JsonValueKind.String
            ? descriptionElement.GetString() ?? string.Empty
            : string.Empty;

        if (!payload.TryGetProperty("scheduledAtUtc", out var scheduledElement) ||
            scheduledElement.ValueKind != JsonValueKind.String ||
            !scheduledElement.TryGetDateTime(out var scheduledAtUtc))
        {
            return InvalidPayload("'scheduledAtUtc'");
        }

        var siteId = payload.TryGetProperty("siteId", out var siteElement) && siteElement.TryGetGuid(out var parsedSiteId)
            ? parsedSiteId
            : (Guid?)null;

        var command = SchedulePreventiveMaintenanceTaskCommand.FromDto(
            tenantId: envelope.TenantId,
            siteId: siteId,
            equipmentId: equipmentId,
            taskDescription: taskDescription,
            scheduledAtUtc: scheduledAtUtc,
            requestedByUserId: envelope.ActorId ?? Guid.Empty,
            correlationId: envelope.CorrelationId,
            causationId: envelope.CausationId ?? envelope.EventId);

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
            $"CMMS maintenance-plan-scheduled payload is missing a valid {missingField}."));

    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
