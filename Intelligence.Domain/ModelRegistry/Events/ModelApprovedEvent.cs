/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelApprovedEvent.cs
    DESCRIPTION:        ApprovedByActorId must always be a human actor — never a system identity.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.ModelRegistry.Events;

/// <summary>Raised when a human reviewer approves a model for production deployment.</summary>
public sealed record ModelApprovedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(ModelApprovedEvent);

    /// <summary>The logical model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>The semantic version approved.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>The human reviewer who approved the model.</summary>
    public string ApprovedByActorId { get; init; } = string.Empty;

    /// <summary>UTC timestamp when approval was granted.</summary>
    public DateTime ApprovedAtUtc { get; init; }
}
