// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseSubmittedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the SafetyCaseValidation lifecycle: submission,
//                  validation, rejection, and expiry. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Events;

/// <summary>Raised when a safety case is submitted for validation.</summary>
public sealed record SafetyCaseSubmittedEvent(
    SafetyCaseValidationEventId EventId,
    SafetyCaseValidationId SafetyCaseValidationId,
    Guid AutonomousVehicleAssetId,
    SafetyStandardReference StandardReference,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyCaseValidationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyCaseSubmitted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a safety case is validated.</summary>
public sealed record SafetyCaseValidatedEvent(
    SafetyCaseValidationEventId EventId,
    SafetyCaseValidationId SafetyCaseValidationId,
    UtcTimestamp ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyCaseValidationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyCaseValidated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a safety case is rejected.</summary>
public sealed record SafetyCaseRejectedEvent(
    SafetyCaseValidationEventId EventId,
    SafetyCaseValidationId SafetyCaseValidationId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyCaseValidationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyCaseRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a validated safety case expires and requires revalidation.</summary>
public sealed record SafetyCaseExpiredEvent(
    SafetyCaseValidationEventId EventId,
    SafetyCaseValidationId SafetyCaseValidationId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyCaseValidationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyCaseExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
