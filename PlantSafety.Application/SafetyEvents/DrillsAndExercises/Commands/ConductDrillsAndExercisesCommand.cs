// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           ConductDrillsAndExercisesCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to record that a scheduled drill was conducted (Scheduled ->
//                  Conducted). Implements IConcurrencyAwareCommand from the start. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;

/// <summary>Records that a scheduled drill was conducted (Scheduled -&gt; Conducted).</summary>
/// <param name="ParticipantCount">Number of participants involved in the drill.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="DrillsAndExercisesId">Identifier of the DrillsAndExercises record.</param>
/// <param name="ConductedByUserId">Identifier of the user performing this operation.</param>
public sealed record ConductDrillsAndExercisesCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid DrillsAndExercisesId,
    Guid ConductedByUserId,
    int ParticipantCount) : ICommand<Result<DrillsAndExercisesStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ConductedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<DrillsAndExercisesStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
