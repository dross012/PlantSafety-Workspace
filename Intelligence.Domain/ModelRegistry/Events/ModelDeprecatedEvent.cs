/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelDeprecatedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.ModelRegistry.Events;

/// <summary>Raised when a registered model is deprecated. DeprecationReason must contain no PII or infrastructure details.</summary>
public sealed record ModelDeprecatedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(ModelDeprecatedEvent);

    /// <summary>The logical model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>The semantic version deprecated.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Sanitized reason for deprecation.</summary>
    public string DeprecationReason { get; init; } = string.Empty;
}
