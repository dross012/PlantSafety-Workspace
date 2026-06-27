/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FindingAddedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.AIInvestigations.Events;

/// <summary>Raised when a finding is added to an investigation.</summary>
public sealed record FindingAddedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(FindingAddedEvent);

    /// <summary>The ID of the finding that was added.</summary>
    public Guid FindingId { get; init; }

    /// <summary>The confidence score of the finding.</summary>
    public double ConfidenceScore { get; init; }

    /// <summary>Whether the confidence score meets the minimum threshold.</summary>
    public bool MeetsThreshold { get; init; }
}
