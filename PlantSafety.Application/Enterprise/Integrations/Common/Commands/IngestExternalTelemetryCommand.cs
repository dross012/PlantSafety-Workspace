// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Common
//  Layer:          Application
//  File:           IngestExternalTelemetryCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.2/1.5 Enterprise System Integrations,
//                  MES/Vendor Telemetry Ingestion)
//  SchemaVersion:  2026.1
//  Description:    Shared inbound telemetry-ingestion command used by BOTH the MES real-time
//                  telemetry adapter (§1.2) and the Vendor IoT gateway/sensor-platform adapter
//                  (§1.5) -- both external surfaces produce the same shape of signal (machine
//                  state/cycle-time/downtime/production-count telemetry), so this one command
//                  covers both rather than duplicating the mapping and detection logic. Reuses the
//                  real Phase 5 Observability.Telemetry value objects and
//                  ProductionTelemetryStreamProcessor (domain anomaly detection -- never
//                  reimplemented here) and, when the telemetry batch reports downtime or a
//                  production/quality delta against a known ProductionRun, dispatches the real
//                  Operational.ProductionTracking.Commands.RecordProductionRunEventCommand (the
//                  same command whose own header documents it as feeding "the real
//                  Observability.Telemetry bottleneck-detection read path"). Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.Telemetry;
using PlantSafety.Application.Operational.ProductionTracking.Commands;
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Enterprise.Integrations.Common.Commands;

/// <summary>One external system's telemetry batch for a single machine, normalized for ingestion.</summary>
/// <param name="TenantId">Tenant identifier resolved from the enveloping <see cref="EnterpriseEventEnvelope"/>.</param>
/// <param name="SourceSystem">The external system this telemetry batch originated from (MES or Vendor).</param>
/// <param name="MachineId">Identity of the machine this telemetry batch pertains to.</param>
/// <param name="ProductionRunId">
/// Identity of the in-progress ProductionRun this telemetry should be recorded against, if the
/// external system correlates telemetry to a specific run. Null when the external system reports
/// only machine-level telemetry with no run correlation -- in that case no
/// <see cref="RecordProductionRunEventCommand"/> is dispatched (there is no real run-level command
/// surface without a RunId), but domain anomaly detection over the batch still runs.
/// </param>
/// <param name="ObservedCycleTimeSeconds">Most recent observed cycle time, in seconds, if reported.</param>
/// <param name="IdealCycleTimeSeconds">Rated/nameplate ideal cycle time, in seconds, for the reporting machine (only meaningful when <paramref name="ObservedCycleTimeSeconds"/> is supplied).</param>
/// <param name="IsDowntime">True when this batch reports a downtime event.</param>
/// <param name="DowntimeReasonCode">Downtime reason code, when <see cref="IsDowntime"/> is true.</param>
/// <param name="DowntimeMinutes">Downtime duration in minutes, when <see cref="IsDowntime"/> is true.</param>
/// <param name="GoodUnitsDelta">Good-unit count reported in this batch.</param>
/// <param name="ScrapUnitsDelta">Scrap/failed-unit count reported in this batch (also used for MES quality-check failures).</param>
/// <param name="BaselineScrapRate">Historical baseline scrap rate (0.0-1.0) used for quality-drift comparison.</param>
/// <param name="RequestedByUserId">Identity of the actor attributed to this ingestion (typically a system/integration actor).</param>
public sealed record IngestExternalTelemetryCommand(
    Guid TenantId,
    EnterpriseSourceSystem SourceSystem,
    Guid MachineId,
    Guid? ProductionRunId,
    double? ObservedCycleTimeSeconds,
    double? IdealCycleTimeSeconds,
    bool IsDowntime,
    string? DowntimeReasonCode,
    double DowntimeMinutes,
    int GoodUnitsDelta,
    int ScrapUnitsDelta,
    double BaselineScrapRate,
    Guid RequestedByUserId) : IRequest<Result<IngestExternalTelemetryResult>>
{
    /// <summary>Correlation identifier for this ingestion (a fresh root operation per inbound batch).</summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Causation identifier -- the inbound envelope's EventId, when known.</summary>
    public Guid? CausationId { get; init; }
}

/// <summary>Result of a successful <see cref="IngestExternalTelemetryCommand"/> execution.</summary>
/// <param name="ProductionRunEventDispatched">True when a <see cref="RecordProductionRunEventCommand"/> was dispatched.</param>
/// <param name="AnomaliesDetected">Count of anomalies the real domain TelemetryAnomalyDetector flagged for this batch.</param>
public sealed record IngestExternalTelemetryResult(bool ProductionRunEventDispatched, int AnomaliesDetected);

/// <summary>Handles <see cref="IngestExternalTelemetryCommand"/>.</summary>
public sealed class IngestExternalTelemetryCommandHandler(
    ISender sender,
    ProductionTelemetryStreamProcessor telemetryStreamProcessor,
    ILogger<IngestExternalTelemetryCommandHandler> logger)
    : IRequestHandler<IngestExternalTelemetryCommand, Result<IngestExternalTelemetryResult>>
{
    /// <inheritdoc/>
    public async Task<Result<IngestExternalTelemetryResult>> Handle(
        IngestExternalTelemetryCommand request, CancellationToken cancellationToken)
    {
        var dispatched = false;
        var nowUtc = UtcTimestamp.From(DateTime.UtcNow);

        if (request.ProductionRunId is { } runId && (request.IsDowntime || request.GoodUnitsDelta != 0 || request.ScrapUnitsDelta != 0))
        {
            var runEventType = request.IsDowntime ? "Downtime" : "TelemetryProductionCount";

            var command = new RecordProductionRunEventCommand(
                CommandId: Guid.NewGuid(),
                CorrelationId: request.CorrelationId,
                CausationId: request.CausationId ?? Guid.NewGuid(),
                TenantId: request.TenantId,
                RunId: runId,
                RunEventType: runEventType,
                Description: $"Telemetry ingested from external {request.SourceSystem} for Machine {request.MachineId}.",
                IsDowntime: request.IsDowntime,
                DowntimeReasonCode: request.DowntimeReasonCode,
                DowntimeMinutes: request.DowntimeMinutes,
                GoodUnitsDelta: request.GoodUnitsDelta,
                ScrapUnitsDelta: request.ScrapUnitsDelta,
                RequestedByUserId: request.RequestedByUserId);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Enterprise.Integrations: RecordProductionRunEventCommand for external telemetry (Machine {MachineId}, Run {RunId}) failed: [{ErrorCode}] {Message}",
                    request.MachineId, runId, result.Error!.ErrorCode, result.Error.Message);
            }
            else
            {
                dispatched = true;
            }
        }

        var batch = new MachineTelemetryBatch
        {
            MachineId = request.MachineId,
            LatestCycleTimeReading = request.ObservedCycleTimeSeconds is { } observedSeconds && request.IdealCycleTimeSeconds is { } idealSeconds
                ? new CycleTimeReading
                {
                    MachineId = request.MachineId,
                    ObservedCycleTime = TimeSpan.FromSeconds(observedSeconds),
                    IdealCycleTime = TimeSpan.FromSeconds(idealSeconds),
                    ObservedAtUtc = nowUtc,
                }
                : null,
            DowntimeEvents = request.IsDowntime
                ?
                [
                    new DowntimeEvent
                    {
                        MachineId = request.MachineId,
                        ReasonCode = request.DowntimeReasonCode ?? "Unknown",
                        StartUtc = UtcTimestamp.From(DateTime.UtcNow.AddMinutes(-request.DowntimeMinutes)),
                        EndUtc = nowUtc,
                    },
                ]
                : [],
            ProductionCountReading = request.GoodUnitsDelta != 0 || request.ScrapUnitsDelta != 0
                ? new ProductionCountReading
                {
                    MachineId = request.MachineId,
                    WindowStartUtc = UtcTimestamp.From(DateTime.UtcNow.AddMinutes(-1)),
                    WindowEndUtc = nowUtc,
                    GoodUnitCount = Math.Max(request.GoodUnitsDelta, 0),
                    ScrapUnitCount = Math.Max(request.ScrapUnitsDelta, 0),
                }
                : null,
            BaselineScrapRate = request.BaselineScrapRate,
        };

        var anomalies = telemetryStreamProcessor.ProcessBatch([batch]);

        foreach (var anomaly in anomalies)
        {
            logger.LogInformation(
                "Enterprise.Integrations: external telemetry from {SourceSystem} for Machine {MachineId} flagged anomaly {AnomalyKind}: {Message}",
                request.SourceSystem, request.MachineId, anomaly.Kind, anomaly.Message);
        }

        return Result<IngestExternalTelemetryResult>.Success(
            new IngestExternalTelemetryResult(dispatched, anomalies.Count));
    }
}
