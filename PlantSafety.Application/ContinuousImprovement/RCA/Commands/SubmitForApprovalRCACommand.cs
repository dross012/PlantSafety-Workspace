// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           SubmitForApprovalRCACommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit an RCA record for approval (InProgress -> PendingApproval).
//                  Implements IConcurrencyAwareCommand from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Submits an RCA record for approval (InProgress -&gt; PendingApproval).</summary>
public sealed record SubmitForApprovalRCACommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RcaId,
    Guid SubmittedByUserId,
    string? Findings) : ICommand<Result<RCAStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => SubmittedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RCAStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
