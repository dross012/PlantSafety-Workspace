/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DataLakeRecord.cs
    DESCRIPTION:        Entity: an immutable ingested record. Corrections require a new record with SupersedesRecordId set.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.DataLake.ValueObjects;

namespace Intelligence.Domain.DataLake.Entities;

/// <summary>A single immutable record ingested into the Intelligence Data Lake.</summary>
public sealed class DataLakeRecord : Entity<Guid>
{
    /// <summary>The data source this record originated from.</summary>
    public DataSource Source { get; private init; }

    /// <summary>Schema version governing the payload structure.</summary>
    public SchemaVersion SchemaVersion { get; private init; }

    /// <summary>Raw JSON payload. Must not contain PII unless explicitly permitted.</summary>
    public string PayloadJson { get; private init; }

    /// <summary>External identifier assigned by the source system. Used for deduplication.</summary>
    public string ExternalRecordId { get; private init; }

    /// <summary>UTC timestamp of the event in the source system.</summary>
    public DateTime EventTimestampUtc { get; private init; }

    /// <summary>UTC timestamp when this record was ingested.</summary>
    public DateTime IngestedAtUtc { get; private init; }

    /// <summary>ID of the record this supersedes, if this is a correction. Null for originals.</summary>
    public Guid? SupersedesRecordId { get; private init; }

    private DataLakeRecord(
        Guid id, DataSource source, SchemaVersion schemaVersion, string payloadJson,
        string externalRecordId, DateTime eventTimestampUtc, Guid? supersedesRecordId)
        : base(id)
    {
        Source = source;
        SchemaVersion = schemaVersion;
        PayloadJson = payloadJson;
        ExternalRecordId = externalRecordId;
        EventTimestampUtc = eventTimestampUtc;
        IngestedAtUtc = DateTime.UtcNow;
        SupersedesRecordId = supersedesRecordId;
    }

    /// <summary>Ingests a new record into the Data Lake.</summary>
    public static DataLakeRecord Ingest(
        DataSource source, SchemaVersion schemaVersion, string payloadJson,
        string externalRecordId, DateTime eventTimestampUtc, Guid? supersedesRecordId = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(schemaVersion);
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("PayloadJson cannot be empty.", nameof(payloadJson));
        if (string.IsNullOrWhiteSpace(externalRecordId))
            throw new ArgumentException("ExternalRecordId cannot be empty.", nameof(externalRecordId));

        return new DataLakeRecord(
            Guid.NewGuid(), source, schemaVersion, payloadJson, externalRecordId.Trim(), eventTimestampUtc, supersedesRecordId);
    }
}
