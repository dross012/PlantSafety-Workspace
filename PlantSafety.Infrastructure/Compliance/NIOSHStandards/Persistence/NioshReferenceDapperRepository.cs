// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.NIOSHStandards
//  Layer:          Infrastructure
//  File:           NioshReferenceDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the NIOSH reference-data read-model
//                  (INioshReferenceRepository). Executes stored procedures for all persistence
//                  operations, mirroring OSHAStandardsDapperRepository's precedent. Global
//                  reference data (chemical hazard profiles, respirator selection, PPE guidance
//                  -- not a regulatory requirement) -- these calls deliberately carry no TenantId
//                  parameter (there is no tenant to scope NIOSH reference data to), so
//                  DapperQueryExecutor's optional TenantContextPropagation enforcement (currently
//                  OFF platform-wide, see TenantContextPropagationOptions) must never be turned on
//                  without excluding this repository's stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Infrastructure.Compliance.NIOSHStandards.Persistence;

/// <summary>Dapper-backed repository for Compliance.NIOSHStandards read-model queries.</summary>
public sealed class NioshReferenceDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<NioshReferenceDapperRepository> logger)
    : INioshReferenceRepository
{
    /// <inheritdoc/>
    public async Task<NioshHazardProfileDto?> GetHazardProfileByCasAsync(string casNumber, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardProfileByCas for NIOSHStandards CAS {CasNumber}", casNumber);

        var parameters = new StoredProcedureBuilder()
            .AddString("CASNumber", casNumber, 50)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardProfileRow>(
            "[Compliance].[usp_NIOSHHazardProfile_GetByCAS]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NioshHazardProfileDto>> SearchHazardsAsync(
        string? keyword, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 400)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<HazardProfileRow>(
            "[Compliance].[usp_NIOSHHazardProfile_Search]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NioshRespiratorMatrixDto>> GetRespiratorRecommendationsAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("HazardId", hazardId).Build();

        var rows = await queryExecutor.QueryAsync<RespiratorMatrixRow>(
            "[Compliance].[usp_NIOSHRespiratorMatrix_GetByHazardId]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NioshPpeGuidanceDto>> GetPpeGuidanceAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("HazardId", hazardId).Build();

        var rows = await queryExecutor.QueryAsync<PpeGuidanceRow>(
            "[Compliance].[usp_NIOSHPPEGuidance_GetByHazardId]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NioshHazardProfileDto>> GetHazardsByModuleAsync(string module, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddString("Module", module, 100).Build();

        var rows = await queryExecutor.QueryAsync<HazardProfileRow>(
            "[Compliance].[usp_NIOSHHazardModuleMapping_GetByModule]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NioshHazardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddDateTime("SinceUtc", sinceUtc).Build();

        var rows = await queryExecutor.QueryAsync<ChangeRow>(
            "[Compliance].[usp_NIOSHHazardChangeLog_GetSince]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static NioshHazardProfileDto ToDto(HazardProfileRow row) => new()
    {
        HazardId = row.HazardId,
        ChemicalName = row.ChemicalName,
        CASNumber = row.CASNumber,
        RelTwa = row.REL_TWA,
        RelStel = row.REL_STEL,
        HealthEffects = row.HealthEffects,
        RecommendedControls = row.RecommendedControls,
        PpeRecommendations = row.PPERecommendations,
        AtmosphericMonitoring = row.AtmosphericMonitoring,
        ErgonomicFactors = row.ErgonomicFactors,
        SourceUrl = row.SourceUrl,
    };

    private static NioshRespiratorMatrixDto ToDto(RespiratorMatrixRow row) => new()
    {
        MatrixId = row.MatrixId,
        HazardId = row.HazardId,
        HazardType = row.HazardType,
        ConcentrationRange = row.ConcentrationRange,
        RecommendedRespirator = row.RecommendedRespirator,
        Notes = row.Notes,
    };

    private static NioshPpeGuidanceDto ToDto(PpeGuidanceRow row) => new()
    {
        GuidanceId = row.GuidanceId,
        HazardId = row.HazardId,
        PPEType = row.PPEType,
        PerformanceCriteria = row.PerformanceCriteria,
        Limitations = row.Limitations,
    };

    private static NioshHazardChangeDto ToDto(ChangeRow row) => new()
    {
        ChangeLogId = row.ChangeLogId,
        HazardId = row.HazardId,
        ChemicalName = row.ChemicalName,
        ChangeType = row.ChangeType,
        DetectedAtUtc = row.DetectedAtUtc,
        Summary = row.Summary,
    };

    private sealed record HazardProfileRow
    {
        public required Guid HazardId { get; init; }
        public required string ChemicalName { get; init; }
        public required string CASNumber { get; init; }
        public double? REL_TWA { get; init; }
        public double? REL_STEL { get; init; }
        public string? HealthEffects { get; init; }
        public string? RecommendedControls { get; init; }
        public string? PPERecommendations { get; init; }
        public string? AtmosphericMonitoring { get; init; }
        public string? ErgonomicFactors { get; init; }
        public string? SourceUrl { get; init; }
    }

    private sealed record RespiratorMatrixRow
    {
        public required Guid MatrixId { get; init; }
        public required Guid HazardId { get; init; }
        public string? HazardType { get; init; }
        public string? ConcentrationRange { get; init; }
        public required string RecommendedRespirator { get; init; }
        public string? Notes { get; init; }
    }

    private sealed record PpeGuidanceRow
    {
        public required Guid GuidanceId { get; init; }
        public required Guid HazardId { get; init; }
        public required string PPEType { get; init; }
        public string? PerformanceCriteria { get; init; }
        public string? Limitations { get; init; }
    }

    private sealed record ChangeRow
    {
        public required Guid ChangeLogId { get; init; }
        public required Guid HazardId { get; init; }
        public required string ChemicalName { get; init; }
        public required string ChangeType { get; init; }
        public required DateTime DetectedAtUtc { get; init; }
        public string? Summary { get; init; }
    }
}
