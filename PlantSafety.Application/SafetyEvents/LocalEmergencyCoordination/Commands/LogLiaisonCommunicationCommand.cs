// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           LogLiaisonCommunicationCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to log a communication exchange with an external responder agency
//                  during a live incident. Implements IConcurrencyAwareCommand. Mirrors
//                  RespondToEmergencyCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Logs a liaison communication exchange with an external responder agency during a live incident.</summary>
public sealed record LogLiaisonCommunicationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LocalEmergencyCoordinationId,
    Guid ExternalResponderLiaisonId,
    string Message,
    Guid LoggedByUserId) : ICommand<Result<LogLiaisonCommunicationResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => LoggedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<LogLiaisonCommunicationResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="LogLiaisonCommunicationCommand"/> execution.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the affected LocalEmergencyCoordination case.</param>
/// <param name="ExternalResponderLiaisonId">Identity of the liaison the communication was logged against.</param>
/// <param name="LiaisonLogEntryId">Identity of the newly recorded log entry.</param>
public sealed record LogLiaisonCommunicationResult(
    Guid LocalEmergencyCoordinationId,
    Guid ExternalResponderLiaisonId,
    Guid LiaisonLogEntryId);
