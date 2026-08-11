// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           CompleteNearMissesCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to complete review of a NearMisses record and record findings
//                  (UnderReview -> Completed). Implements IConcurrencyAwareCommand from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Commands;

/// <summary>Completes review of a NearMisses record and records findings (UnderReview -&gt; Completed).</summary>
/// <param name="Findings">Free-text findings recorded when the review is completed.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="NearMissesId">Identifier of the NearMisses record.</param>
/// <param name="CompletedByUserId">Identifier of the user performing this operation.</param>
public sealed record CompleteNearMissesCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid NearMissesId,
    Guid CompletedByUserId,
    string? Findings) : ICommand<Result<NearMissesStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => CompletedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<NearMissesStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
