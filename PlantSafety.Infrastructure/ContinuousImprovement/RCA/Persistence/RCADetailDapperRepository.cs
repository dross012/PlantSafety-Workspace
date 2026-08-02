// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Infrastructure
//  File:           RCADetailDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer read-only
//                  IRCADetailRepository (backs GetRCAByIdQuery). Shares the same
//                  usp_RCA_GetById stored procedure used by the Domain-layer write-side
//                  RCAAggregateDapperRepository.GetByIdAsync, but projects the flat row directly
//                  into RCADetailDto rather than reconstructing the aggregate -- a plain read
//                  model with no domain-object rehydration. Distinct from the pre-existing,
//                  untouched RCADapperRepository (backs the date-range GetRCAQuery via
//                  usp_RCA_GetRcaRecords). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.RCA.Models;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

namespace PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence;

/// <summary>Dapper-backed repository for a single RCA read-model record by identity.</summary>
public sealed class RCADetailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RCADetailDapperRepository> logger)
    : IRCADetailRepository
{
    /// <inheritdoc/>
    public async Task<RCADetailDto?> GetByIdAsync(Guid rcaId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for RCA read-model {RcaId} Tenant {TenantId}", rcaId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RcaId", rcaId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RCARow>(
            "[ContinuousImprovement].[usp_RCA_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return new RCADetailDto
        {
            RcaId = row.RcaId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            ProblemStatement = row.ProblemStatement,
            Methodology = row.Methodology,
            Status = row.Status,
            InitiatedAtUtc = row.InitiatedAtUtc,
            OwnerUserId = row.OwnerUserId,
            StartedBy = row.StartedBy,
            StartedAtUtc = row.StartedAtUtc,
            SubmittedBy = row.SubmittedBy,
            Findings = row.Findings,
            SubmittedAtUtc = row.SubmittedAtUtc,
            ClosedBy = row.ClosedBy,
            RootCauseSummary = row.RootCauseSummary,
            ClosedAtUtc = row.ClosedAtUtc,
            ArchivedAtUtc = row.ArchivedAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedBy = row.CreatedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat read-model row shape returned by <c>usp_RCA_GetById</c>.</summary>
    private sealed record RCARow
    {
        public string? ETag { get; init; }

        public required Guid RcaId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ProblemStatement { get; init; }
        public required string Methodology { get; init; }
        public required string Status { get; init; }
        public required DateTime InitiatedAtUtc { get; init; }
        public required Guid OwnerUserId { get; init; }
        public Guid? StartedBy { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        public Guid? SubmittedBy { get; init; }
        public string? Findings { get; init; }
        public DateTime? SubmittedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public string? RootCauseSummary { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
