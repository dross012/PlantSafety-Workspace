/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IDataLakeRecordRepository.cs
    DESCRIPTION:        Repository contract for DataLakeRecord entities. DataLakeRecord is not an aggregate
                        root (it is immutable after ingestion and owned by the Data Lake subsystem), so
                        this interface does not extend IRepository<,>. The application layer uses this to
                        check for duplicates and retrieve records for transformation pipelines.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.DataLake.Entities;
using Intelligence.Domain.DataLake.ValueObjects;

namespace Intelligence.Domain.DataLake.Interfaces;

/// <summary>Persistence contract for <see cref="DataLakeRecord"/> entities.</summary>
public interface IDataLakeRecordRepository
{
    /// <summary>Returns the record with the given identifier, or null when not found.</summary>
    Task<DataLakeRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all records for the given source and external record ID.
    /// Used for deduplication checks before ingesting new records.
    /// </summary>
    Task<IReadOnlyList<DataLakeRecord>> GetByExternalIdAsync(
        DataSource source, string externalRecordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns records ingested within the specified UTC time window for the given source.
    /// Used by transformation pipelines to select batches for processing.
    /// </summary>
    Task<IReadOnlyList<DataLakeRecord>> GetBySourceAndWindowAsync(
        DataSource source, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ingested record. Committed when the Unit of Work completes.</summary>
    Task AddAsync(DataLakeRecord record, CancellationToken cancellationToken = default);
}
