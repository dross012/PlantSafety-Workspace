// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           StartProcessingDataLakeActivityCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to begin processing of an ingested data lake activity (Ingested ->
//                  Processing). Implements IConcurrencyAwareCommand from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.DataLake.Commands;

/// <summary>Begins processing of an ingested data lake activity (Ingested -&gt; Processing).</summary>
public sealed record StartProcessingDataLakeActivityCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid DataLakeActivityId,
    Guid StartedByUserId) : ICommand<Result<DataLakeActivityStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => StartedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<DataLakeActivityStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
