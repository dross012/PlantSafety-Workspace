/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               SafetyAlertRaisedEvent.cs
    DESCRIPTION:        SAFETY-CRITICAL domain event raised when a safety alert is triggered from a prediction
                        that has passed confidence threshold and dual validation (for High/Critical risk levels).
                        This event indicates that human review is required. It must never be raised for
                        predictions below the confidence floor. (Rule 2.13, Safety Rules 4.4/4.5)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.PredictiveSafety.Events;

/// <summary>
/// SAFETY-CRITICAL: Raised when a safety alert is triggered by a validated prediction.
/// This event drives human escalation workflows and must only be raised after all safety gates pass.
/// </summary>
public sealed record SafetyAlertRaisedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(SafetyAlertRaisedEvent);

    /// <summary>The prediction subject (plant area, process, or equipment).</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>The risk level that triggered this alert.</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>The confidence score of the prediction that raised this alert. Always >= minimum threshold.</summary>
    public double ConfidenceScore { get; init; }

    /// <summary>The prediction horizon in hours.</summary>
    public int HorizonHours { get; init; }

    /// <summary>The model that generated the underlying prediction.</summary>
    public string ModelName { get; init; } = string.Empty;
}
