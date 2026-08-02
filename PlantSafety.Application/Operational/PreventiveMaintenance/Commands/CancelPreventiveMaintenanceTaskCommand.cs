// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           CancelPreventiveMaintenanceTaskCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to cancel a PreventiveMaintenanceTask before completion (Scheduled or
//                  InProgress -> Cancelled). Implements IConcurrencyAwareCommand from the start.
//                  Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.RejectPermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Cancels a PreventiveMaintenanceTask before completion (Scheduled/InProgress -&gt; Cancelled).</summary>
public sealed record CancelPreventiveMaintenanceTaskCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PreventiveMaintenanceTaskId,
    Guid CancelledByUserId,
    string Reason) : ICommand<Result<PreventiveMaintenanceTaskStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => CancelledByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PreventiveMaintenanceTaskStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
