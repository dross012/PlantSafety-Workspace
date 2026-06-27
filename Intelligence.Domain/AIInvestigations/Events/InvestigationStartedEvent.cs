/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationStartedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.AIInvestigations.Events;

/// <summary>Raised when a new AI investigation is opened.</summary>
public sealed record InvestigationStartedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(InvestigationStartedEvent);

    /// <summary>The investigation subject.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>The actor who requested the investigation.</summary>
    public string RequestedByActorId { get; init; } = string.Empty;
}
