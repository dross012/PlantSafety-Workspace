// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           RecordProductionRunEventCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to record a timeline event against an in-progress ProductionRun --
//                  covers Step/Pause/Error/Resume/Complete-adjacent manual entries as well as
//                  downtime events (IsDowntime=true), the latter feeding the real
//                  Observability.Telemetry bottleneck-detection read path. Implements
//                  IConcurrencyAwareCommand: callers should supply the ETag captured from a prior
//                  read. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Records a timeline event against an in-progress ProductionRun.</summary>
public sealed record RecordProductionRunEventCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RunId,
    string RunEventType,
    string Description,
    bool IsDowntime,
    string? DowntimeReasonCode,
    double DowntimeMinutes,
    int GoodUnitsDelta,
    int ScrapUnitsDelta,
    Guid RequestedByUserId) : ICommand<Result<ProductionRunEventResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ProductionRunEventResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="RecordProductionRunEventCommand"/> execution.</summary>
/// <param name="ProductionRunId">Identity of the affected ProductionRun aggregate.</param>
/// <param name="GoodUnitCount">Running total of good units after this event.</param>
/// <param name="ScrapUnitCount">Running total of scrapped units after this event.</param>
public sealed record ProductionRunEventResult(Guid ProductionRunId, int GoodUnitCount, int ScrapUnitCount);
