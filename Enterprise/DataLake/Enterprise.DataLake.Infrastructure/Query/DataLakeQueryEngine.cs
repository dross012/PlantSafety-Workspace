// ============================================================================
//  Project:        Enterprise.DataLake.Infrastructure
//  Module:         Query
//  File:           DataLakeQueryEngine.cs
//  Created:        2026-08-08
//  Description:    Phase D.2: deterministic read/query API over
//                  IDataLakeStore -- by type, by partition, by UTC date
//                  range, and per-type counts. No aggregation beyond plain
//                  filtering/counting; anything more (trends, forecasts)
//                  belongs to Phase D.8's predictive layer, not this one.
// ============================================================================
using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Storage;

namespace Enterprise.DataLake.Infrastructure.Query;

/// <summary>Deterministic read/query API over the enterprise data lake.</summary>
public interface IDataLakeQueryEngine
{
    /// <summary>Returns every record of the given type.</summary>
    IReadOnlyList<DataLakeRecord> QueryByType(DataLakeRecordType recordType);

    /// <summary>Returns every record in the given partition.</summary>
    IReadOnlyList<DataLakeRecord> QueryByPartition(DataLakePartitionKey partitionKey);

    /// <summary>Returns every record of the given type recorded within [<paramref name="fromUtc"/>, <paramref name="toUtc"/>] inclusive.</summary>
    IReadOnlyList<DataLakeRecord> QueryByDateRange(DataLakeRecordType recordType, DateOnly fromUtc, DateOnly toUtc);

    /// <summary>Returns the total record count per <see cref="DataLakeRecordType"/>, including types with zero records.</summary>
    IReadOnlyDictionary<DataLakeRecordType, int> CountByType();
}

/// <summary><see cref="IDataLakeQueryEngine"/> implementation over <see cref="IDataLakeStore"/>.</summary>
public sealed class DataLakeQueryEngine(IDataLakeStore store) : IDataLakeQueryEngine
{
    /// <inheritdoc />
    public IReadOnlyList<DataLakeRecord> QueryByType(DataLakeRecordType recordType) => store.GetByType(recordType);

    /// <inheritdoc />
    public IReadOnlyList<DataLakeRecord> QueryByPartition(DataLakePartitionKey partitionKey) => store.GetByPartition(partitionKey);

    /// <inheritdoc />
    public IReadOnlyList<DataLakeRecord> QueryByDateRange(DataLakeRecordType recordType, DateOnly fromUtc, DateOnly toUtc) =>
        store.GetByType(recordType)
            .Where(r => r.PartitionKey.PartitionDate >= fromUtc && r.PartitionKey.PartitionDate <= toUtc)
            .ToArray();

    /// <inheritdoc />
    public IReadOnlyDictionary<DataLakeRecordType, int> CountByType()
    {
        IReadOnlyList<DataLakeRecord> all = store.GetAll();
        return Enum.GetValues<DataLakeRecordType>()
            .ToDictionary(type => type, type => all.Count(r => r.RecordType == type));
    }
}
