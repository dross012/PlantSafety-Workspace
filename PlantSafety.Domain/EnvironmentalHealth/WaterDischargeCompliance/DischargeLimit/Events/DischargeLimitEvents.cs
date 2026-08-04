// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the DischargeLimit lifecycle: definition, revision,
//                  retirement. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Events;

/// <summary>Raised when a new discharge limit is defined.</summary>
public sealed record DischargeLimitDefinedEvent(
    DischargeLimitEventId EventId,
    DischargeLimitId DischargeLimitId,
    Guid DischargePointId,
    DischargeParameterType Parameter,
    decimal LimitValue,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeLimitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeLimitDefined";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a discharge limit value is revised.</summary>
public sealed record DischargeLimitRevisedEvent(
    DischargeLimitEventId EventId,
    DischargeLimitId DischargeLimitId,
    decimal NewLimitValue,
    int RevisionCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeLimitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeLimitRevised";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a discharge limit is retired.</summary>
public sealed record DischargeLimitRetiredEvent(
    DischargeLimitEventId EventId,
    DischargeLimitId DischargeLimitId,
    string? Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeLimitDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeLimitRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}