// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           ApproveModelCommand.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL (Safety Rule 4.5) command to approve a PendingApproval model
//                  registry entry for production deployment (PendingApproval -> Approved).
//                  ApprovedByUserId must always be a human actor -- the handler constructs the
//                  domain actor as PerformedByActor.User(...), never System()/AiAgent(), enforcing
//                  the aggregate's human-approver invariant. Implements IConcurrencyAwareCommand
//                  from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Commands;

/// <summary>SAFETY-CRITICAL: Approves a PendingApproval model registry entry for production deployment (Safety Rule 4.5).</summary>
public sealed record ApproveModelCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid ModelRegistryEntryId,
    Guid ApprovedByUserId) : ICommand<Result<ModelRegistryEntryStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ApprovedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ModelRegistryEntryStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
