// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           CreateWorkOrderCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to create a new WorkOrder. Carries the full command envelope
//                  (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Operational.PreventiveMaintenance.Commands
//                  .SchedulePreventiveMaintenanceTaskCommand. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Creates a new WorkOrder.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="WorkOrderCode">Human-readable work order code (format WO-###).</param>
/// <param name="MachineId">Identifier of the machine/equipment targeted by this work order.</param>
/// <param name="MachineCode">Human-readable machine code (format M-###).</param>
/// <param name="Priority">Priority classification (Low, Medium, High, Critical).</param>
/// <param name="Description">Description of the work to be performed.</param>
/// <param name="DueAtUtc">UTC instant the work order is due.</param>
/// <param name="RequestedByUserId">Identity of the user creating the work order.</param>
public sealed record CreateWorkOrderCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string WorkOrderCode,
    Guid MachineId,
    string MachineCode,
    string Priority,
    string Description,
    DateTime DueAtUtc,
    Guid RequestedByUserId) : ICommand<Result<CreateWorkOrderResult>>
{
    /// <summary>Identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateWorkOrderResult>>.CausationId => CausationId;
}

/// <summary>Result of a successful <see cref="CreateWorkOrderCommand"/> execution.</summary>
/// <param name="WorkOrderId">Identity of the newly created WorkOrder aggregate.</param>
/// <param name="WorkOrderCode">Human-readable work order code.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateWorkOrderResult(Guid WorkOrderId, string WorkOrderCode, string Status);
