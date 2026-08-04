// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           AssignExternalResponderLiaisonCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to assign an internal user as the live-incident liaison to a specific
//                  external local emergency responder agency. Implements IConcurrencyAwareCommand.
//                  Mirrors RespondToEmergencyCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Assigns an internal user as the live-incident liaison to a specific external responder agency.</summary>
public sealed record AssignExternalResponderLiaisonCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LocalEmergencyCoordinationId,
    Guid? IncidentId,
    string AgencyName,
    Guid LiaisonUserId,
    Guid AssignedByUserId) : ICommand<Result<AssignExternalResponderLiaisonResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => AssignedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AssignExternalResponderLiaisonResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="AssignExternalResponderLiaisonCommand"/> execution.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the affected LocalEmergencyCoordination case.</param>
/// <param name="ExternalResponderLiaisonId">Identity of the newly assigned liaison.</param>
public sealed record AssignExternalResponderLiaisonResult(
    Guid LocalEmergencyCoordinationId,
    Guid ExternalResponderLiaisonId);
