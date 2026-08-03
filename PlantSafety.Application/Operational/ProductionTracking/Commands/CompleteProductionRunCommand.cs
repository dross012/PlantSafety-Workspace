// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           CompleteProductionRunCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to end an in-progress ProductionRun, either successfully (Completed) or
//                  abnormally (Failed). Implements IConcurrencyAwareCommand: callers should supply
//                  the ETag captured from a prior read. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Ends an in-progress ProductionRun (Completed or Failed).</summary>
public sealed record CompleteProductionRunCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RunId,
    bool Succeeded,
    Guid RequestedByUserId) : ICommand<Result<ProductionRunStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ProductionRunStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Shared result payload for ProductionRun commands that only need to echo back the resulting identity/status.</summary>
/// <param name="ProductionRunId">Identity of the affected ProductionRun aggregate.</param>
/// <param name="Status">Lifecycle status after the command executed.</param>
public sealed record ProductionRunStatusResult(Guid ProductionRunId, string Status);
