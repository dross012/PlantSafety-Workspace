/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               SafetyPredictionGeneratedEvent.cs
    DESCRIPTION:        SAFETY-CRITICAL domain event raised when a safety prediction is published.
                        Carries the confidence score and MeetsMinimumThreshold flag so all downstream
                        consumers can apply the confidence gate independently (Safety Rule 4.4/4.5).
                        Never carries raw model weights, connection strings, or PII. (Rule 2.13)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. MeetsMinimumThreshold must never be removed.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.PredictiveSafety.Events;

/// <summary>
/// SAFETY-CRITICAL: Raised when a safety prediction is published.
/// <see cref="MeetsMinimumThreshold"/> must be checked by all consumers before taking automated action.
/// </summary>
public sealed record SafetyPredictionGeneratedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(SafetyPredictionGeneratedEvent);

    /// <summary>The prediction subject (plant area, process, or equipment).</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>The assessed risk level string.</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>The prediction confidence score.</summary>
    public double ConfidenceScore { get; init; }

    /// <summary>
    /// SAFETY-CRITICAL: Whether the confidence score meets the minimum threshold for automated actions.
    /// Consumers must not act on this prediction when this is false (Safety Rule 4.4/4.5).
    /// </summary>
    public bool MeetsMinimumThreshold { get; init; }

    /// <summary>The prediction horizon in hours.</summary>
    public int HorizonHours { get; init; }

    /// <summary>The model name that generated this prediction.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>The model version that generated this prediction.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>The number of evidence items attached to this prediction.</summary>
    public int EvidenceCount { get; init; }
}
