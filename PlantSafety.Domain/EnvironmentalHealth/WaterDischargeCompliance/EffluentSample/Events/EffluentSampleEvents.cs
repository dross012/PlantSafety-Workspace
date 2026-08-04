// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the EffluentSample lifecycle: recording,
//                  append-only correction, and voiding. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Events;

/// <summary>Raised when a new effluent sample is recorded.</summary>
public sealed record EffluentSampleRecordedEvent(
    EffluentSampleEventId EventId,
    EffluentSampleId EffluentSampleId,
    Guid DischargePointId,
    SampleParameterType Parameter,
    decimal MeasuredValue,
    string UnitOfMeasure,
    string LabSourceSystem,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEffluentSampleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EffluentSampleRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a sample is superseded by an append-only correction record.</summary>
public sealed record EffluentSampleCorrectedEvent(
    EffluentSampleEventId EventId,
    EffluentSampleId EffluentSampleId,
    Guid CorrectingSampleId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEffluentSampleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EffluentSampleCorrected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a sample is voided.</summary>
public sealed record EffluentSampleVoidedEvent(
    EffluentSampleEventId EventId,
    EffluentSampleId EffluentSampleId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEffluentSampleDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EffluentSampleVoided";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}