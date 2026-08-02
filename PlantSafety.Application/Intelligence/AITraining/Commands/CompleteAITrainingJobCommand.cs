// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           CompleteAITrainingJobCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to mark a training job as completed successfully (Running -> Completed).
//                  Implements IConcurrencyAwareCommand from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AITraining.Commands;

/// <summary>Marks a training job as completed successfully (Running -&gt; Completed).</summary>
public sealed record CompleteAITrainingJobCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid AITrainingJobId,
    Guid PerformedByUserId,
    string? ModelArtifactReference) : ICommand<Result<AITrainingJobStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AITrainingJobStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
