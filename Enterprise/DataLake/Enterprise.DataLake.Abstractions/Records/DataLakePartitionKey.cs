// ============================================================================
//  Project:        Enterprise.DataLake
//  Module:         Abstractions.Records
//  File:           DataLakePartitionKey.cs
//  Created:        2026-08-08
//  Description:    Phase D.2: the deterministic partition scheme for the
//                  data lake -- every record is partitioned by its
//                  RecordType and the UTC calendar date it was recorded,
//                  mirroring the coarse-grained, type-then-date partitioning
//                  convention common to append-only event stores. No
//                  invented sharding scheme beyond what these two fields
//                  already determine.
// ============================================================================
using Enterprise.DataLake.Abstractions.Enums;

namespace Enterprise.DataLake.Abstractions.Records;

/// <summary>A data lake record's partition: its record type plus the UTC calendar date it was recorded.</summary>
public readonly record struct DataLakePartitionKey(DataLakeRecordType RecordType, DateOnly PartitionDate)
{
    /// <summary>Derives the partition key for a record type recorded at the given UTC instant.</summary>
    public static DataLakePartitionKey For(DataLakeRecordType recordType, DateTime recordedAtUtc) =>
        new(recordType, DateOnly.FromDateTime(recordedAtUtc));

    /// <summary>A deterministic, human-readable partition path, e.g. "Facility/2026-08-08".</summary>
    public string ToPartitionPath() => $"{RecordType}/{PartitionDate:yyyy-MM-dd}";
}
