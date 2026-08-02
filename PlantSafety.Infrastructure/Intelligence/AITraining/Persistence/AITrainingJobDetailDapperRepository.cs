// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Infrastructure
//  File:           AITrainingJobDetailDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer read-only
//                  IAITrainingJobDetailRepository (backs GetAITrainingJobByIdQuery). Shares the
//                  same usp_AITrainingJob_GetById stored procedure used by the Domain-layer
//                  write-side AITrainingJobAggregateDapperRepository.GetByIdAsync, but projects the
//                  flat row directly into AITrainingJobDetailDto rather than reconstructing the
//                  aggregate -- a plain read model with no domain-object rehydration. Distinct from
//                  the pre-existing, untouched AITrainingDapperRepository (backs the date-range
//                  GetAITrainingQuery via usp_AITraining_GetTrainingJobs). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AITraining.Models;
using PlantSafety.Application.Intelligence.AITraining.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.AITraining.Persistence;

/// <summary>Dapper-backed repository for a single AITrainingJob read-model record by identity.</summary>
public sealed class AITrainingJobDetailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AITrainingJobDetailDapperRepository> logger)
    : IAITrainingJobDetailRepository
{
    /// <inheritdoc/>
    public async Task<AITrainingJobDetailDto?> GetByIdAsync(Guid aiTrainingJobId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for AITrainingJob read-model {AITrainingJobId} Tenant {TenantId}", aiTrainingJobId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AITrainingJobId", aiTrainingJobId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AITrainingJobRow>(
            "[Intelligence].[usp_AITrainingJob_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return new AITrainingJobDetailDto
        {
            AITrainingJobId = row.AITrainingJobId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            ModelName = row.ModelName,
            ModelVersion = row.ModelVersion,
            DatasetReference = row.DatasetReference,
            Status = row.Status,
            StartedAtUtc = row.StartedAtUtc,
            InitiatedBy = row.InitiatedBy,
            RunStartedBy = row.RunStartedBy,
            RunStartedAtUtc = row.RunStartedAtUtc,
            ModelArtifactReference = row.ModelArtifactReference,
            CompletedAtUtc = row.CompletedAtUtc,
            FailureReason = row.FailureReason,
            FailedAtUtc = row.FailedAtUtc,
            ArchivedAtUtc = row.ArchivedAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedBy = row.CreatedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AITrainingJob_GetById</c>.</summary>
    private sealed record AITrainingJobRow
    {
        public string? ETag { get; init; }

        public required Guid AITrainingJobId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ModelName { get; init; }
        public required string ModelVersion { get; init; }
        public string? DatasetReference { get; init; }
        public required string Status { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public required Guid InitiatedBy { get; init; }
        public Guid? RunStartedBy { get; init; }
        public DateTime? RunStartedAtUtc { get; init; }
        public string? ModelArtifactReference { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public string? FailureReason { get; init; }
        public DateTime? FailedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
