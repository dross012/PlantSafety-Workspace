// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Infrastructure
//  File:           ModelRegistryEntryDetailDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer read-only
//                  IModelRegistryEntryDetailRepository (backs GetModelRegistryEntryByIdQuery).
//                  Shares the same usp_ModelRegistryEntry_GetById stored procedure used by the
//                  Domain-layer write-side ModelRegistryEntryAggregateDapperRepository.GetByIdAsync,
//                  but projects the flat row directly into ModelRegistryEntryDetailDto rather than
//                  reconstructing the aggregate. Mirrors
//                  PlantSafety.Infrastructure.Intelligence.DataLake.Persistence
//                  .DataLakeActivityDetailDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.ModelRegistry.Models;
using PlantSafety.Application.Intelligence.ModelRegistry.Repositories;
using PlantSafety.Domain.Intelligence.ModelRegistry.Services;

namespace PlantSafety.Infrastructure.Intelligence.ModelRegistry.Persistence;

/// <summary>Dapper-backed repository for a single ModelRegistryEntry read-model record by identity.</summary>
public sealed class ModelRegistryEntryDetailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ModelRegistryEntryDetailDapperRepository> logger)
    : IModelRegistryEntryDetailRepository
{
    /// <inheritdoc/>
    public async Task<ModelRegistryEntryDetailDto?> GetByIdAsync(Guid modelRegistryEntryId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for ModelRegistryEntry read-model {ModelRegistryEntryId} Tenant {TenantId}", modelRegistryEntryId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ModelRegistryEntryId", modelRegistryEntryId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<ModelRegistryEntryRow>(
            "[Intelligence].[usp_ModelRegistryEntry_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return new ModelRegistryEntryDetailDto
        {
            ModelRegistryEntryId = row.ModelRegistryEntryId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            ModelName = row.ModelName,
            ModelVersion = row.ModelVersion,
            Status = row.Status,
            DatasetHash = row.DatasetHash,
            TrainingAccuracyScore = row.TrainingAccuracyScore,
            MeetsMinimumConfidenceThreshold = row.TrainingAccuracyScore >= ModelRegistryDomainService.MinimumConfidenceScore,
            RegisteredBy = row.RegisteredBy,
            ApprovedBy = row.ApprovedBy,
            ApprovedAtUtc = row.ApprovedAtUtc,
            DeprecationReason = row.DeprecationReason,
            DeprecatedAtUtc = row.DeprecatedAtUtc,
            RetiredAtUtc = row.RetiredAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedBy = row.CreatedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ModelRegistryEntry_GetById</c>.</summary>
    private sealed record ModelRegistryEntryRow
    {
        public string? ETag { get; init; }

        public required Guid ModelRegistryEntryId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ModelName { get; init; }
        public required string ModelVersion { get; init; }
        public required string Status { get; init; }
        public required string DatasetHash { get; init; }
        public required double TrainingAccuracyScore { get; init; }
        public required Guid RegisteredBy { get; init; }
        public Guid? ApprovedBy { get; init; }
        public DateTime? ApprovedAtUtc { get; init; }
        public string? DeprecationReason { get; init; }
        public DateTime? DeprecatedAtUtc { get; init; }
        public DateTime? RetiredAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
