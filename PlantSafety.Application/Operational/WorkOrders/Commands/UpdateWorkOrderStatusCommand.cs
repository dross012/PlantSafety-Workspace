// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           UpdateWorkOrderStatusCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Generic lifecycle-transition command covering Start, PutOnHold, Resume, and
//                  Cancel -- the transitions that are pure status moves with no additional payload
//                  beyond an optional reason. Complete is handled by the distinct
//                  CompleteWorkOrderCommand because it also carries completion notes. TargetStatus
//                  is validated against PlantSafety.Domain.Operational.WorkOrders.Enums
//                  .WorkOrderStatus server-side by the handler; invalid transitions surface as a
//                  Domain-severity Result failure via the aggregate's own guard
//                  (WorkOrderInvalidStatusTransitionException). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Applies a Start/PutOnHold/Resume/Cancel lifecycle transition to an existing WorkOrder.</summary>
public sealed record UpdateWorkOrderStatusCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid WorkOrderId,
    string TargetStatus,
    string? Reason,
    Guid RequestedByUserId) : ICommand<Result<WorkOrderStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkOrderStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
