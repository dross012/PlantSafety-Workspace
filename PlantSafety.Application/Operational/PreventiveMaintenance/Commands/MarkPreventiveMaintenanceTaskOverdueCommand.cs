// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           MarkPreventiveMaintenanceTaskOverdueCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to mark a scheduled PreventiveMaintenanceTask overdue after it passes
//                  its due date without starting/completing (Scheduled -> Overdue). Implements
//                  IConcurrencyAwareCommand from the start. No approver identity is required by
//                  the domain method (PerformedByActor.System is used since this transition is
//                  typically system-triggered by a scheduled job), but the command still carries
//                  an ActorId for the envelope/audit trail, supplied by the caller (e.g. a
//                  background job's service-account UserId). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ExpirePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Marks a scheduled PreventiveMaintenanceTask overdue (Scheduled -&gt; Overdue).</summary>
public sealed record MarkPreventiveMaintenanceTaskOverdueCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PreventiveMaintenanceTaskId,
    Guid PerformedByUserId) : ICommand<Result<PreventiveMaintenanceTaskStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PreventiveMaintenanceTaskStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
