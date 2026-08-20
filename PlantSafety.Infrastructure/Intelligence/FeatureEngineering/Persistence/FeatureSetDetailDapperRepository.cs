// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Infrastructure
//  File:           FeatureSetDetailDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer read-only
//                  IFeatureSetDetailRepository (backs GetFeatureSetByIdQuery). Shares the same
//                  usp_FeatureSet_GetById stored procedure used by the Domain-layer write-side
//                  FeatureSetAggregateDapperRepository.GetByIdAsync, but projects the flat row
//                  directly into FeatureSetDetailDto rather than reconstructing the entity. Mirrors
//                  PlantSafety.Infrastructure.Intelligence.DataLake.Persistence
//                  .DataLakeActivityDetailDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.FeatureEngineering.Models;
using PlantSafety.Application.Intelligence.FeatureEngineering.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.FeatureEngineering.Persistence;

/// <summary>Dapper-backed repository for a single FeatureSet read-model record by identity.</summary>
public sealed class FeatureSetDetailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<FeatureSetDetailDapperRepository> logger)
    : IFeatureSetDetailRepository
{
    /// <inheritdoc/>
    public async Task<FeatureSetDetailDto?> GetByIdAsync(Guid featureSetId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for FeatureSet read-model {FeatureSetId} Tenant {TenantId}", featureSetId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FeatureSetId", featureSetId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<FeatureSetRow>(
            "[Intelligence].[usp_FeatureSet_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var featureNamesCsv = row.FeatureNamesCsv ?? string.Empty;
        var featureCount = featureNamesCsv.Length == 0
            ? 0
            : featureNamesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

        return new FeatureSetDetailDto
        {
            FeatureSetId = row.FeatureSetId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            Name = row.Name,
            Version = row.Version,
            DataSourcePath = row.DataSourcePath,
            IsPublished = row.IsPublished,
            FeatureNamesCsv = featureNamesCsv,
            FeatureCount = featureCount,
            ComputedAtUtc = row.ComputedAtUtc,
            PublishedAtUtc = row.PublishedAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedBy = row.CreatedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat read-model row shape returned by <c>usp_FeatureSet_GetById</c>.</summary>
    private sealed record FeatureSetRow
    {
        public string? ETag { get; init; }

        public required Guid FeatureSetId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Name { get; init; }
        public required string Version { get; init; }
        public required string DataSourcePath { get; init; }
        public required bool IsPublished { get; init; }
        public string? FeatureNamesCsv { get; init; }
        public required DateTime ComputedAtUtc { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
