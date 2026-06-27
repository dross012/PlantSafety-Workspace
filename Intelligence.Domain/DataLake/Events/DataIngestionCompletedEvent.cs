/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DataIngestionCompletedEvent.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.DataLake.Events;

/// <summary>Raised when a data ingestion run completes. Triggers downstream feature engineering pipelines.</summary>
public sealed record DataIngestionCompletedEvent : DomainEvent
{
    /// <inheritdoc />
    public override string EventType => nameof(DataIngestionCompletedEvent);

    /// <summary>The source system name that produced the ingested data.</summary>
    public string SourceSystemName { get; init; } = string.Empty;

    /// <summary>The data category ingested.</summary>
    public string DataCategory { get; init; } = string.Empty;

    /// <summary>The schema version of the ingested records.</summary>
    public string SchemaVersion { get; init; } = string.Empty;

    /// <summary>The number of records successfully ingested.</summary>
    public int RecordCount { get; init; }

    /// <summary>UTC timestamp of the earliest event in the ingested batch.</summary>
    public DateTime BatchStartUtc { get; init; }

    /// <summary>UTC timestamp of the latest event in the ingested batch.</summary>
    public DateTime BatchEndUtc { get; init; }
}
