// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Mes
//  Layer:          Application
//  File:           MesProductionOrderUpdateInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.2 Enterprise System Integrations, MES)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound MES production-order-update event
//                  ("mes.productionorder.statuschanged.v1") into the platform's real, public
//                  Operational.ProductionTracking commands: "Started" dispatches
//                  StartProductionRunCommand (a new ProductionRun); "Completed"/"Failed" dispatches
//                  CompleteProductionRunCommand against an existing RunId. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Operational.ProductionTracking.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Mes.Translators;

/// <summary>Translates an inbound MES production-order-update event into ProductionTracking commands.</summary>
public sealed class MesProductionOrderUpdateInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "mes.productionorder.statuschanged.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Mes &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("targetStatus", out var statusElement) ||
            statusElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(statusElement.GetString()))
        {
            return InvalidPayload("'targetStatus'");
        }

        var targetStatus = statusElement.GetString()!;

        if (string.Equals(targetStatus, "Started", StringComparison.OrdinalIgnoreCase))
            return await HandleStartAsync(envelope, cancellationToken);

        return await HandleCompleteAsync(envelope, targetStatus, cancellationToken);
    }

    private async Task<Result<bool>> HandleStartAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("lineId", out var lineIdElement) || !lineIdElement.TryGetGuid(out var lineId))
            return InvalidPayload("'lineId'");

        if (!payload.TryGetProperty("machineId", out var machineIdElement) || !machineIdElement.TryGetGuid(out var machineId))
            return InvalidPayload("'machineId'");

        var runCode = GetString(payload, "orderCode") ?? Guid.NewGuid().ToString("N")[..12];
        var lineName = GetString(payload, "productionLineName") ?? "External MES Line";
        var operatorName = GetString(payload, "operatorName") ?? "Unknown";
        var idealCycleTimeSeconds = payload.TryGetProperty("idealCycleTimeSeconds", out var idealElement) && idealElement.TryGetDouble(out var ideal)
            ? ideal
            : 60.0;

        var siteId = payload.TryGetProperty("siteId", out var siteElement) && siteElement.TryGetGuid(out var parsedSiteId)
            ? parsedSiteId
            : (Guid?)null;

        var command = new StartProductionRunCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: "v1",
            TenantId: envelope.TenantId,
            SiteId: siteId,
            RunCode: runCode,
            LineId: lineId,
            LineName: lineName,
            MachineId: machineId,
            OperatorName: operatorName,
            IdealCycleTimeSeconds: idealCycleTimeSeconds,
            RequestedByUserId: envelope.ActorId ?? Guid.Empty);

        var result = await sender.Send(command, cancellationToken);

        return ToSharedKernelResult(result.IsSuccess, result.IsSuccess ? null : result.Error);
    }

    private async Task<Result<bool>> HandleCompleteAsync(EnterpriseEventEnvelope envelope, string targetStatus, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("productionRunId", out var runIdElement) || !runIdElement.TryGetGuid(out var runId))
            return InvalidPayload("'productionRunId'");

        var succeeded = !string.Equals(targetStatus, "Failed", StringComparison.OrdinalIgnoreCase);

        var command = new CompleteProductionRunCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            TenantId: envelope.TenantId,
            RunId: runId,
            Succeeded: succeeded,
            RequestedByUserId: envelope.ActorId ?? Guid.Empty);

        var result = await sender.Send(command, cancellationToken);

        return ToSharedKernelResult(result.IsSuccess, result.IsSuccess ? null : result.Error);
    }

    private static string? GetString(JsonElement payload, string propertyName) =>
        payload.TryGetProperty(propertyName, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;

    private static Result<bool> ToSharedKernelResult(bool isSuccess, ApplicationError? error) =>
        isSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(error!.ErrorCode)),
                ErrorType.Failure,
                error.Message));

    private static Result<bool> InvalidPayload(string missingField) =>
        Result<bool>.Failure(new Error(
            ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
            ErrorType.Validation,
            $"MES production-order-update payload is missing a valid {missingField}."));

    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
