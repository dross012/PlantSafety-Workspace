// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the LimitExceedance lifecycle: detection, response
//                  initiation, response closure. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Events;

/// <summary>Raised when a discharge limit exceedance is detected/recorded.</summary>
public sealed record DischargeLimitExceededEvent(
    LimitExceedanceEventId EventId,
    LimitExceedanceId LimitExceedanceId,
    Guid DischargePointId,
    string ParameterName,
    decimal MeasuredValue,
    decimal LimitValue,
    decimal ExceedancePercentage,
    ExceedanceSeverity Severity,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILimitExceedanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeLimitExceeded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an exceedance response workflow is initiated.</summary>
public sealed record ExceedanceResponseInitiatedEvent(
    LimitExceedanceEventId EventId,
    LimitExceedanceId LimitExceedanceId,
    Guid InitiatedByUserId,
    string? Notes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILimitExceedanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ExceedanceResponseInitiated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an exceedance response is closed.</summary>
public sealed record ExceedanceResponseClosedEvent(
    LimitExceedanceEventId EventId,
    LimitExceedanceId LimitExceedanceId,
    Guid ClosedByUserId,
    string? ResolutionNotes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILimitExceedanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ExceedanceResponseClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}