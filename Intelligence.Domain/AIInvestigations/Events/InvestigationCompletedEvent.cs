/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationCompletedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.AIInvestigations.Events;

/// <summary>Raised when a human reviewer completes an investigation.</summary>
public sealed record InvestigationCompletedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(InvestigationCompletedEvent);

    /// <summary>The total number of findings at completion time.</summary>
    public int FindingCount { get; init; }

    /// <summary>The reviewer who completed the investigation.</summary>
    public string ReviewerActorId { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the investigation was completed.</summary>
    public DateTime CompletedAtUtc { get; init; }
}
