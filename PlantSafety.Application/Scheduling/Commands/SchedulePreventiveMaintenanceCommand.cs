// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to place a preventive maintenance task on the PM calendar with a due
// date ("Preventive Maintenance (PM) Scheduling" - PM calendar).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Schedules a preventive maintenance task with a due date.</summary>
public sealed record SchedulePreventiveMaintenanceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PMSchedulingId,
    DateTime DueAtUtc,
    Guid PerformedByUserId) : ICommand<Result<PMSchedulingActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PMSchedulingActionResult>>.CausationId => CausationId;
}
