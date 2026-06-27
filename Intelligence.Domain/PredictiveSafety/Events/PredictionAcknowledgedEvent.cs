/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               PredictionAcknowledgedEvent.cs
    DESCRIPTION:        Domain event raised when a human actor acknowledges a safety prediction.
                        Acknowledgement is terminal — this event permanently closes the prediction lifecycle.
                        No automated process may raise or reverse an acknowledgement (Safety Rule 4.7).
                        (Rule 2.13)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. AcknowledgedByActorId must never be removed.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.PredictiveSafety.Events;

/// <summary>
/// Raised when a human actor acknowledges a safety prediction. Terminal event — once raised,
/// no automated process may undo the human decision (Safety Rule 4.7).
/// </summary>
public sealed record PredictionAcknowledgedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(PredictionAcknowledgedEvent);

    /// <summary>The prediction subject that was acknowledged.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>The risk level of the acknowledged prediction.</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>The actor ID of the human who acknowledged this prediction.</summary>
    public string AcknowledgedByActorId { get; init; } = string.Empty;

    /// <summary>UTC timestamp when acknowledgement was recorded.</summary>
    public DateTime AcknowledgedAtUtc { get; init; }
}
