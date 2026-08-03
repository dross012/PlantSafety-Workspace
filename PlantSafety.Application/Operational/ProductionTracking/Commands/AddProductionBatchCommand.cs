// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           AddProductionBatchCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to add a Batch to an in-progress ProductionRun. Implements
//                  IConcurrencyAwareCommand: callers should supply the ETag captured from a prior
//                  read. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Adds a Batch to an in-progress ProductionRun.</summary>
public sealed record AddProductionBatchCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RunId,
    string BatchCode,
    string Status,
    Guid RequestedByUserId) : ICommand<Result<AddProductionBatchResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AddProductionBatchResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="AddProductionBatchCommand"/> execution.</summary>
/// <param name="BatchId">Identity of the newly added batch.</param>
/// <param name="ProductionRunId">Identity of the ProductionRun the batch was added to.</param>
/// <param name="BatchCode">Human-readable batch code.</param>
public sealed record AddProductionBatchResult(Guid BatchId, Guid ProductionRunId, string BatchCode);
