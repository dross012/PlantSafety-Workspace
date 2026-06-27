/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DataTransformationAppliedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.DataLake.Events;

/// <summary>Raised when a schema transformation is applied to raw Data Lake records.</summary>
public sealed record DataTransformationAppliedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(DataTransformationAppliedEvent);

    /// <summary>The name of the transformation applied.</summary>
    public string TransformationName { get; init; } = string.Empty;

    /// <summary>The source schema version transformed from.</summary>
    public string FromSchemaVersion { get; init; } = string.Empty;

    /// <summary>The target schema version transformed to.</summary>
    public string ToSchemaVersion { get; init; } = string.Empty;

    /// <summary>The number of records affected.</summary>
    public int AffectedRecordCount { get; init; }
}
