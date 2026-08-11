// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           CompleteObservationCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to record completion of the required follow-up/corrective action
//                  (Active -> Completed). Implements IConcurrencyAwareCommand from the start.
//                  Mirrors PlantSafety.Application.Compliance.Permits.Commands.CompletePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Observation.Commands;

/// <summary>Records completion of the required follow-up/corrective action (Active -&gt; Completed).</summary>
/// <param name="ResolutionNotes">Optional free-text notes describing how the corrective action was resolved.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="ObservationId">Identifier of the Observation record.</param>
/// <param name="CompletedByUserId">Identifier of the user performing this operation.</param>
public sealed record CompleteObservationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid ObservationId,
    Guid CompletedByUserId,
    string? ResolutionNotes) : ICommand<Result<ObservationStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => CompletedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ObservationStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
