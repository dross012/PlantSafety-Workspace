// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           EstablishMutualAidAgreementCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to establish a mutual aid agreement with an external local emergency
//                  responder agency under an existing LocalEmergencyCoordination case. Implements
//                  IConcurrencyAwareCommand: callers should supply the ETag captured from a prior
//                  read; a stale value results in ConcurrencyConflictException. Mirrors
//                  RespondToEmergencyCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Establishes a mutual aid agreement with an external local emergency responder agency.</summary>
public sealed record EstablishMutualAidAgreementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LocalEmergencyCoordinationId,
    string AgencyName,
    string Discipline,
    int DocumentVersion,
    DateTime EffectiveFromUtc,
    DateTime RenewalDueAtUtc,
    Guid EstablishedByUserId) : ICommand<Result<EstablishMutualAidAgreementResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => EstablishedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<EstablishMutualAidAgreementResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="EstablishMutualAidAgreementCommand"/> execution.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the affected LocalEmergencyCoordination case.</param>
/// <param name="MutualAidAgreementId">Identity of the newly established mutual aid agreement.</param>
/// <param name="RenewalStatus">Renewal/currency status computed at establishment time.</param>
public sealed record EstablishMutualAidAgreementResult(
    Guid LocalEmergencyCoordinationId,
    Guid MutualAidAgreementId,
    string RenewalStatus);
