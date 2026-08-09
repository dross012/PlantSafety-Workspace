// ============================================================================
//  Project:        Enterprise.DataLake.Infrastructure
//  Module:         Storage
//  File:           DataLakeStore.cs
//  Created:        2026-08-08
//  Description:    Phase D.2: the append-only, thread-safe, in-memory,
//                  process-local backing store for the enterprise data
//                  lake. Deliberately append-only -- there is no Update or
//                  Delete, matching a real data lake's write-once
//                  semantics; correcting a record means appending a new
//                  one, never mutating history. Mirrors Shared.FacilityDomain's
//                  FacilityStore ConcurrentDictionary-backed pattern, but
//                  backed by a ConcurrentQueue since insertion order (not
//                  keyed lookup/overwrite) is what append-only storage needs.
// ============================================================================
using System.Collections.Concurrent;

using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;

namespace Enterprise.DataLake.Infrastructure.Storage;

/// <summary>Append-only, thread-safe access to persisted <see cref="DataLakeRecord"/>s.</summary>
public interface IDataLakeStore
{
    /// <summary>Appends a record to the lake. Records are never updated or removed once appended.</summary>
    void Append(DataLakeRecord record);

    /// <summary>Returns every persisted record, in append order.</summary>
    IReadOnlyList<DataLakeRecord> GetAll();

    /// <summary>Returns every persisted record with the given partition key, in append order.</summary>
    IReadOnlyList<DataLakeRecord> GetByPartition(DataLakePartitionKey partitionKey);

    /// <summary>Returns every persisted record of the given type, in append order.</summary>
    IReadOnlyList<DataLakeRecord> GetByType(DataLakeRecordType recordType);
}

/// <summary>Append-only, thread-safe, in-memory, process-local <see cref="IDataLakeStore"/> implementation.</summary>
public sealed class DataLakeStore : IDataLakeStore
{
    private readonly ConcurrentQueue<DataLakeRecord> _records = new();

    /// <inheritdoc />
    public void Append(DataLakeRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _records.Enqueue(record);
    }

    /// <inheritdoc />
    public IReadOnlyList<DataLakeRecord> GetAll() => _records.ToArray();

    /// <inheritdoc />
    public IReadOnlyList<DataLakeRecord> GetByPartition(DataLakePartitionKey partitionKey) =>
        _records.Where(r => r.PartitionKey == partitionKey).ToArray();

    /// <inheritdoc />
    public IReadOnlyList<DataLakeRecord> GetByType(DataLakeRecordType recordType) =>
        _records.Where(r => r.RecordType == recordType).ToArray();
}
