// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           ApprovePermitsCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to approve and issue a Permits request (Created -> Active). Implements
//                  IConcurrencyAwareCommand from the start: callers should supply the ETag
//                  captured from a prior read; a stale value results in
//                  ConcurrencyConflictException. Mirrors
//                  PlantSafety.Application.PermitToWork.Commands.ApprovePermitWorkflowCommand /
//                  IssuePermitWorkflowCommand (validity-hours shape).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Approves and issues a Permits request (Created -&gt; Active).</summary>
/// <param name="ValidityHours">Optional validity window, in hours, from approval. Null means no computed expiration.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="PermitsId">Identifier of the Permits record.</param>
/// <param name="ApprovedByUserId">Identifier of the user performing this operation.</param>
public sealed record ApprovePermitsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitsId,
    Guid ApprovedByUserId,
    int? ValidityHours) : ICommand<Result<PermitsStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ApprovedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitsStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
