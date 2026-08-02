// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Infrastructure
//  File:           DataLakeActivityDetailDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer read-only
//                  IDataLakeActivityDetailRepository (backs GetDataLakeActivityByIdQuery). Shares
//                  the same usp_DataLakeActivity_GetById stored procedure used by the Domain-layer
//                  write-side DataLakeActivityAggregateDapperRepository.GetByIdAsync, but projects
//                  the flat row directly into DataLakeActivityDetailDto rather than reconstructing
//                  the aggregate -- a plain read model with no domain-object rehydration. Distinct
//                  from the pre-existing, untouched DataLakeDapperRepository (backs the date-range
//                  GetDataLakeQuery via usp_DataLake_GetDataLakeRecords). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.DataLake.Models;
using PlantSafety.Application.Intelligence.DataLake.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.DataLake.Persistence;

/// <summary>Dapper-backed repository for a single DataLakeActivity read-model record by identity.</summary>
public sealed class DataLakeActivityDetailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<DataLakeActivityDetailDapperRepository> logger)
    : IDataLakeActivityDetailRepository
{
    /// <inheritdoc/>
    public async Task<DataLakeActivityDetailDto?> GetByIdAsync(Guid dataLakeActivityId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for DataLakeActivity read-model {DataLakeActivityId} Tenant {TenantId}", dataLakeActivityId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DataLakeActivityId", dataLakeActivityId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<DataLakeActivityRow>(
            "[Intelligence].[usp_DataLakeActivity_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return new DataLakeActivityDetailDto
        {
            DataLakeActivityId = row.DataLakeActivityId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            DatasetName = row.DatasetName,
            Zone = row.Zone,
            RecordCount = row.RecordCount,
            Status = row.Status,
            ProcessedAtUtc = row.ProcessedAtUtc,
            ProcessedBy = row.ProcessedBy,
            ProcessingStartedBy = row.ProcessingStartedBy,
            ProcessingStartedAtUtc = row.ProcessingStartedAtUtc,
            CuratedAtUtc = row.CuratedAtUtc,
            FailureReason = row.FailureReason,
            FailedAtUtc = row.FailedAtUtc,
            ArchivedAtUtc = row.ArchivedAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedBy = row.CreatedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat read-model row shape returned by <c>usp_DataLakeActivity_GetById</c>.</summary>
    private sealed record DataLakeActivityRow
    {
        public string? ETag { get; init; }

        public required Guid DataLakeActivityId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string DatasetName { get; init; }
        public required string Zone { get; init; }
        public long? RecordCount { get; init; }
        public required string Status { get; init; }
        public required DateTime ProcessedAtUtc { get; init; }
        public required Guid ProcessedBy { get; init; }
        public Guid? ProcessingStartedBy { get; init; }
        public DateTime? ProcessingStartedAtUtc { get; init; }
        public DateTime? CuratedAtUtc { get; init; }
        public string? FailureReason { get; init; }
        public DateTime? FailedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
