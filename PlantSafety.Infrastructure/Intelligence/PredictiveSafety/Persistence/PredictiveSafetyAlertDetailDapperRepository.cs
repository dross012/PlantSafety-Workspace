// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Infrastructure
//  File:           PredictiveSafetyAlertDetailDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer read-only
//                  IPredictiveSafetyAlertDetailRepository (backs
//                  GetPredictiveSafetyAlertByIdQuery). Shares the same
//                  usp_PredictiveSafetyAlert_GetById stored procedure used by the Domain-layer
//                  write-side PredictiveSafetyAlertAggregateDapperRepository.GetByIdAsync, but
//                  projects the flat row directly into PredictiveSafetyAlertDetailDto rather than
//                  reconstructing the aggregate -- a plain read model with no domain-object
//                  rehydration. Distinct from the pre-existing, untouched
//                  PredictiveSafetyDapperRepository (backs the date-range
//                  GetPredictiveSafetyQuery via usp_PredictiveSafety_GetPredictions).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.PredictiveSafety.Persistence;

/// <summary>Dapper-backed repository for a single PredictiveSafetyAlert read-model record by identity.</summary>
public sealed class PredictiveSafetyAlertDetailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveSafetyAlertDetailDapperRepository> logger)
    : IPredictiveSafetyAlertDetailRepository
{
    /// <inheritdoc/>
    public async Task<PredictiveSafetyAlertDetailDto?> GetByIdAsync(Guid predictiveSafetyAlertId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for PredictiveSafetyAlert read-model {PredictiveSafetyAlertId} Tenant {TenantId}", predictiveSafetyAlertId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveSafetyAlertId", predictiveSafetyAlertId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PredictiveSafetyAlertRow>(
            "[Intelligence].[usp_PredictiveSafetyAlert_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return new PredictiveSafetyAlertDetailDto
        {
            PredictiveSafetyAlertId = row.PredictiveSafetyAlertId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            RiskCategory = row.RiskCategory,
            PredictedRiskScore = row.PredictedRiskScore,
            ModelVersion = row.ModelVersion,
            Status = row.Status,
            PredictedAtUtc = row.PredictedAtUtc,
            GeneratedBy = row.GeneratedBy,
            AcknowledgedBy = row.AcknowledgedBy,
            AcknowledgedAtUtc = row.AcknowledgedAtUtc,
            ResolvedBy = row.ResolvedBy,
            ResolutionNotes = row.ResolutionNotes,
            ResolvedAtUtc = row.ResolvedAtUtc,
            ArchivedAtUtc = row.ArchivedAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedBy = row.CreatedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PredictiveSafetyAlert_GetById</c>.</summary>
    private sealed record PredictiveSafetyAlertRow
    {
        public string? ETag { get; init; }

        public required Guid PredictiveSafetyAlertId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string RiskCategory { get; init; }
        public required double PredictedRiskScore { get; init; }
        public string? ModelVersion { get; init; }
        public required string Status { get; init; }
        public required DateTime PredictedAtUtc { get; init; }
        public required Guid GeneratedBy { get; init; }
        public Guid? AcknowledgedBy { get; init; }
        public DateTime? AcknowledgedAtUtc { get; init; }
        public Guid? ResolvedBy { get; init; }
        public string? ResolutionNotes { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
