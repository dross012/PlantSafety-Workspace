// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           AssignWorkOrderCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to (re)assign a technician to an existing WorkOrder. Implements
//                  IConcurrencyAwareCommand from the start: callers should supply the ETag captured
//                  from a prior read; a stale value results in ConcurrencyConflictException.
//                  Mirrors PlantSafety.Application.Operational.PreventiveMaintenance.Commands
//                  .StartPreventiveMaintenanceTaskCommand. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>(Re)assigns a technician to an existing WorkOrder.</summary>
public sealed record AssignWorkOrderCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid WorkOrderId,
    Guid TechnicianId,
    string TechnicianName,
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
