/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FeatureSetPublishedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.FeatureEngineering.Events;

/// <summary>Raised when a feature set is published to the feature store. Triggers downstream training preparation.</summary>
public sealed record FeatureSetPublishedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(FeatureSetPublishedEvent);

    /// <summary>The name of the published feature set.</summary>
    public string FeatureSetName { get; init; } = string.Empty;

    /// <summary>The version of the published feature set.</summary>
    public string FeatureSetVersion { get; init; } = string.Empty;

    /// <summary>The number of features in this set.</summary>
    public int FeatureCount { get; init; }

    /// <summary>UTC timestamp when published.</summary>
    public DateTime PublishedAtUtc { get; init; }
}
