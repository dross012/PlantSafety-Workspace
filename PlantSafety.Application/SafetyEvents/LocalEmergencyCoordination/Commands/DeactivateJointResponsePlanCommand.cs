// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           DeactivateJointResponsePlanCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to deactivate a previously activated joint response plan. Implements
//                  IConcurrencyAwareCommand. Mirrors CloseEmergencyCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Deactivates a previously activated joint response plan.</summary>
public sealed record DeactivateJointResponsePlanCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LocalEmergencyCoordinationId,
    Guid JointResponsePlanId,
    Guid DeactivatedByUserId) : ICommand<Result<JointResponsePlanStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => DeactivatedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<JointResponsePlanStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
