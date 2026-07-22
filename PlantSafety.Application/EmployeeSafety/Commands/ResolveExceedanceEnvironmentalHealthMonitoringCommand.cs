// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to ResolveExceedance (lifecycle transition) for EnvironmentalHealthMonitoring ("EmployeeSafety.EnvironmentalHealthMonitoring").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Invokes <c>EnvironmentalHealthMonitoring.ResolveExceedance</c> on an existing EnvironmentalHealthMonitoring aggregate.</summary>
public sealed record ResolveExceedanceEnvironmentalHealthMonitoringCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid EnvironmentalHealthMonitoringId,
    decimal ReMeasuredScore,
    Guid PerformedByUserId) : ICommand<Result<EnvironmentalHealthMonitoringActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EnvironmentalHealthMonitoringActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
