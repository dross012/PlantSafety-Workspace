/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJobFailedEvent.cs
    DESCRIPTION:        SIEM-safe: SanitizedReason must not contain stack traces, secrets, or PII.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.AITraining.Events;

/// <summary>Raised when a training job fails. SanitizedReason must contain no secrets or stack traces.</summary>
public sealed record TrainingJobFailedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(TrainingJobFailedEvent);

    /// <summary>The model name for the failed training job.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>A sanitized, human-readable reason for the failure.</summary>
    public string SanitizedReason { get; init; } = string.Empty;
}
