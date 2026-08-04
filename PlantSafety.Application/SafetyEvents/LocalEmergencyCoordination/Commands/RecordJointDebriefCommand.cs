// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           RecordJointDebriefCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to record a post-incident debrief conducted jointly with external
//                  local emergency responder agencies. Implements IConcurrencyAwareCommand.
//                  Mirrors RespondToEmergencyCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Records a post-incident debrief conducted jointly with external local emergency responder agencies.</summary>
public sealed record RecordJointDebriefCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LocalEmergencyCoordinationId,
    Guid? IncidentId,
    string AgenciesInvolved,
    string Summary,
    Guid RecordedByUserId) : ICommand<Result<RecordJointDebriefResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RecordedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordJointDebriefResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="RecordJointDebriefCommand"/> execution.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the affected LocalEmergencyCoordination case.</param>
/// <param name="JointDebriefRecordId">Identity of the newly recorded debrief.</param>
public sealed record RecordJointDebriefResult(
    Guid LocalEmergencyCoordinationId,
    Guid JointDebriefRecordId);
