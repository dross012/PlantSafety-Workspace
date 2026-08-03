// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           StartProductionRunCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to start a new ProductionRun against a registered ProductionLine and a
//                  telemetry-monitored machine. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Starts a new ProductionRun.</summary>
public sealed record StartProductionRunCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string RunCode,
    Guid LineId,
    string LineName,
    Guid MachineId,
    string OperatorName,
    double IdealCycleTimeSeconds,
    Guid RequestedByUserId) : ICommand<Result<StartProductionRunResult>>
{
    /// <summary>Identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<StartProductionRunResult>>.CausationId => CausationId;
}

/// <summary>Result of a successful <see cref="StartProductionRunCommand"/> execution.</summary>
/// <param name="ProductionRunId">Identity of the newly started ProductionRun aggregate.</param>
/// <param name="RunCode">Human-readable run code.</param>
/// <param name="Status">Lifecycle status of the newly started run.</param>
public sealed record StartProductionRunResult(Guid ProductionRunId, string RunCode, string Status);
