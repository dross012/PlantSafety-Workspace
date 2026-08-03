// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Mes
//  Layer:          Application
//  File:           MesTelemetryInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.2 Enterprise System Integrations, MES)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound MES real-time telemetry event
//                  ("mes.telemetry.reported.v1" -- machine state, downtime, cycle-time, production
//                  count) into the shared IngestExternalTelemetryCommand, reusing the real Phase 5
//                  Observability.Telemetry value objects/ProductionTelemetryStreamProcessor rather
//                  than reinventing telemetry value objects (see that command's own header).
//                  Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Enterprise.Integrations.Common.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Mes.Translators;

/// <summary>Translates an inbound MES telemetry event into <see cref="IngestExternalTelemetryCommand"/>.</summary>
public sealed class MesTelemetryInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "mes.telemetry.reported.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Mes &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("machineId", out var machineIdElement) || !machineIdElement.TryGetGuid(out var machineId))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
                ErrorType.Validation,
                "MES telemetry payload is missing a valid 'machineId'."));
        }

        var productionRunId = payload.TryGetProperty("productionRunId", out var runIdElement) && runIdElement.TryGetGuid(out var parsedRunId)
            ? parsedRunId
            : (Guid?)null;

        var observedCycleTimeSeconds = payload.TryGetProperty("observedCycleTimeSeconds", out var observedElement) && observedElement.TryGetDouble(out var observed)
            ? observed
            : (double?)null;

        var idealCycleTimeSeconds = payload.TryGetProperty("idealCycleTimeSeconds", out var idealElement) && idealElement.TryGetDouble(out var ideal)
            ? ideal
            : (double?)null;

        var isDowntime = payload.TryGetProperty("isDowntime", out var isDowntimeElement) &&
            isDowntimeElement.ValueKind is JsonValueKind.True or JsonValueKind.False &&
            isDowntimeElement.GetBoolean();

        var downtimeReasonCode = payload.TryGetProperty("downtimeReasonCode", out var reasonElement) && reasonElement.ValueKind == JsonValueKind.String
            ? reasonElement.GetString()
            : null;

        var downtimeMinutes = payload.TryGetProperty("downtimeMinutes", out var downtimeMinutesElement) && downtimeMinutesElement.TryGetDouble(out var minutes)
            ? minutes
            : 0.0;

        var goodUnitsDelta = payload.TryGetProperty("goodUnitsDelta", out var goodElement) && goodElement.TryGetInt32(out var good) ? good : 0;
        var scrapUnitsDelta = payload.TryGetProperty("scrapUnitsDelta", out var scrapElement) && scrapElement.TryGetInt32(out var scrap) ? scrap : 0;
        var baselineScrapRate = payload.TryGetProperty("baselineScrapRate", out var baselineElement) && baselineElement.TryGetDouble(out var baseline)
            ? baseline
            : 0.0;

        var command = new IngestExternalTelemetryCommand(
            TenantId: envelope.TenantId,
            SourceSystem: EnterpriseSourceSystem.Mes,
            MachineId: machineId,
            ProductionRunId: productionRunId,
            ObservedCycleTimeSeconds: observedCycleTimeSeconds,
            IdealCycleTimeSeconds: idealCycleTimeSeconds,
            IsDowntime: isDowntime,
            DowntimeReasonCode: downtimeReasonCode,
            DowntimeMinutes: downtimeMinutes,
            GoodUnitsDelta: goodUnitsDelta,
            ScrapUnitsDelta: scrapUnitsDelta,
            BaselineScrapRate: baselineScrapRate,
            RequestedByUserId: envelope.ActorId ?? Guid.Empty)
        {
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId ?? envelope.EventId,
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(result.Error.ErrorCode)),
                ErrorType.Failure,
                result.Error.Message));
    }

    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
