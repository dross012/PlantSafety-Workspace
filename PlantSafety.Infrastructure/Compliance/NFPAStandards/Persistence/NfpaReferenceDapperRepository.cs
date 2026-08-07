// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.NFPAStandards
//  Layer:          Infrastructure
//  File:           NfpaReferenceDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the NFPA hazard-profile read-model
//                  (INfpaReferenceRepository). Executes stored procedures for all persistence
//                  operations, mirroring OSHAStandardsDapperRepository's precedent. Global
//                  reference data -- these calls deliberately carry no TenantId parameter (there
//                  is no tenant to scope NFPA hazard-guidance text to), so DapperQueryExecutor's
//                  optional TenantContextPropagation enforcement (currently OFF platform-wide,
//                  see TenantContextPropagationOptions) must never be turned on without excluding
//                  this repository's stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;

namespace PlantSafety.Infrastructure.Compliance.NFPAStandards.Persistence;

/// <summary>Dapper-backed repository for Compliance.NFPAStandards read-model queries.</summary>
public sealed class NfpaReferenceDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<NfpaReferenceDapperRepository> logger)
    : INfpaReferenceRepository
{
    /// <inheritdoc/>
    public async Task<NfpaHazardProfileDto?> GetHazardProfileAsync(string codeNumber, string sectionNumber, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardProfile for NFPAStandards Code {CodeNumber} Section {SectionNumber}", codeNumber, sectionNumber);

        var parameters = new StoredProcedureBuilder()
            .AddString("CodeNumber", codeNumber, 20)
            .AddString("SectionNumber", sectionNumber, 50)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardRow>(
            "[Compliance].[usp_NfpaHazards_GetByCodeSection]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaHazardProfileDto>> SearchHazardsAsync(
        string? keyword, string? codeNumber, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 200)
            .AddString("CodeNumber", codeNumber, 20)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<HazardRow>(
            "[Compliance].[usp_NfpaHazards_Search]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaControlMatrixDto>> GetControlsForHazardAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("HazardId", hazardId).Build();

        var rows = await queryExecutor.QueryAsync<ControlRow>(
            "[Compliance].[usp_NfpaControls_GetByHazardId]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaHazardModuleDto>> GetHazardsByModuleAsync(string moduleName, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddString("ModuleName", moduleName, 100).Build();

        var rows = await queryExecutor.QueryAsync<HazardModuleRow>(
            "[Compliance].[usp_NfpaHazards_GetByModule]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaHazardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddDateTime("SinceUtc", sinceUtc).Build();

        var rows = await queryExecutor.QueryAsync<ChangeRow>(
            "[Compliance].[usp_NfpaHazardChangeLog_GetSince]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static NfpaHazardProfileDto ToDto(HazardRow row) => new()
    {
        HazardId = row.HazardId,
        CodeNumber = row.CodeNumber,
        SectionNumber = row.SectionNumber,
        HazardName = row.HazardName,
        HazardDescription = row.HazardDescription,
        RecommendedControls = row.RecommendedControls,
        PPERecommendations = row.PPERecommendations,
        ElectricalSafetyGuidance = row.ElectricalSafetyGuidance,
        HotWorkGuidance = row.HotWorkGuidance,
        FlammableMaterialsGuidance = row.FlammableMaterialsGuidance,
        SourceUrl = row.SourceUrl,
        CreatedOn = row.CreatedOn,
        SectionTextHash = row.SectionTextHash,
    };

    private static NfpaControlMatrixDto ToDto(ControlRow row) => new()
    {
        MatrixId = row.MatrixId,
        HazardId = row.HazardId,
        ControlType = row.ControlType,
        ControlDescription = row.ControlDescription,
        Severity = row.Severity,
        Rationale = row.Rationale,
    };

    private static NfpaHazardModuleDto ToDto(HazardModuleRow row) => new()
    {
        HazardId = row.HazardId,
        CodeNumber = row.CodeNumber,
        SectionNumber = row.SectionNumber,
        HazardName = row.HazardName,
        HazardDescription = row.HazardDescription,
        RecommendedControls = row.RecommendedControls,
        PPERecommendations = row.PPERecommendations,
        ElectricalSafetyGuidance = row.ElectricalSafetyGuidance,
        HotWorkGuidance = row.HotWorkGuidance,
        FlammableMaterialsGuidance = row.FlammableMaterialsGuidance,
        SourceUrl = row.SourceUrl,
        CreatedOn = row.CreatedOn,
        SectionTextHash = row.SectionTextHash,
        MappingId = row.MappingId,
        MappingRuleCode = row.MappingRuleCode,
        Rationale = row.Rationale,
        Severity = row.Severity,
    };

    private static NfpaHazardChangeDto ToDto(ChangeRow row) => new()
    {
        ChangeLogId = row.ChangeLogId,
        HazardId = row.HazardId,
        CodeNumber = row.CodeNumber,
        SectionNumber = row.SectionNumber,
        ChangeType = row.ChangeType,
        PreviousTextHash = row.PreviousTextHash,
        NewTextHash = row.NewTextHash,
        DetectedAtUtc = row.DetectedAtUtc,
        Summary = row.Summary,
    };

    private sealed record HazardRow
    {
        public required Guid HazardId { get; init; }
        public required string CodeNumber { get; init; }
        public required string SectionNumber { get; init; }
        public required string HazardName { get; init; }
        public required string HazardDescription { get; init; }
        public string? RecommendedControls { get; init; }
        public string? PPERecommendations { get; init; }
        public string? ElectricalSafetyGuidance { get; init; }
        public string? HotWorkGuidance { get; init; }
        public string? FlammableMaterialsGuidance { get; init; }
        public string? SourceUrl { get; init; }
        public required DateTime CreatedOn { get; init; }
        public string? SectionTextHash { get; init; }
    }

    private sealed record ControlRow
    {
        public required Guid MatrixId { get; init; }
        public required Guid HazardId { get; init; }
        public required string ControlType { get; init; }
        public required string ControlDescription { get; init; }
        public required string Severity { get; init; }
        public required string Rationale { get; init; }
    }

    private sealed record HazardModuleRow
    {
        public required Guid HazardId { get; init; }
        public required string CodeNumber { get; init; }
        public required string SectionNumber { get; init; }
        public required string HazardName { get; init; }
        public required string HazardDescription { get; init; }
        public string? RecommendedControls { get; init; }
        public string? PPERecommendations { get; init; }
        public string? ElectricalSafetyGuidance { get; init; }
        public string? HotWorkGuidance { get; init; }
        public string? FlammableMaterialsGuidance { get; init; }
        public string? SourceUrl { get; init; }
        public required DateTime CreatedOn { get; init; }
        public string? SectionTextHash { get; init; }
        public required Guid MappingId { get; init; }
        public required string MappingRuleCode { get; init; }
        public required string Rationale { get; init; }
        public required string Severity { get; init; }
    }

    private sealed record ChangeRow
    {
        public required Guid ChangeLogId { get; init; }
        public required Guid HazardId { get; init; }
        public required string CodeNumber { get; init; }
        public required string SectionNumber { get; init; }
        public required string ChangeType { get; init; }
        public string? PreviousTextHash { get; init; }
        public string? NewTextHash { get; init; }
        public required DateTime DetectedAtUtc { get; init; }
        public string? Summary { get; init; }
    }
}
