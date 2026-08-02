// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           SchedulePreventiveMaintenanceTaskCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to schedule a new PreventiveMaintenanceTask. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Schedules a new PreventiveMaintenanceTask.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EquipmentId">Identifier of the equipment scheduled for maintenance.</param>
/// <param name="TaskDescription">Description of the maintenance task.</param>
/// <param name="ScheduledAtUtc">UTC instant the task is scheduled for.</param>
/// <param name="RequestedByUserId">Identity of the user scheduling the task.</param>
public sealed record SchedulePreventiveMaintenanceTaskCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid EquipmentId,
    string TaskDescription,
    DateTime ScheduledAtUtc,
    Guid RequestedByUserId) : ICommand<Result<SchedulePreventiveMaintenanceTaskResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SchedulePreventiveMaintenanceTaskResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="SchedulePreventiveMaintenanceTaskCommand"/> from raw, untrusted transport values.</summary>
    public static SchedulePreventiveMaintenanceTaskCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid equipmentId,
        string taskDescription,
        DateTime scheduledAtUtc,
        Guid requestedByUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            EquipmentId: equipmentId,
            TaskDescription: (taskDescription ?? string.Empty).Trim(),
            ScheduledAtUtc: scheduledAtUtc,
            RequestedByUserId: requestedByUserId);
}
