// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.FMCSAStandards
//  Layer:          Infrastructure
//  File:           FmcsaStandardsDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the FMCSA standards read-model
//                  (IFmcsaStandardsRepository). Executes stored procedures for all persistence
//                  operations, mirroring OSHAStandardsDapperRepository's precedent exactly. Global
//                  reference data -- these calls deliberately carry no TenantId parameter (there
//                  is no tenant to scope FMCSA regulatory text to). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Infrastructure.Compliance.FMCSAStandards.Persistence;

/// <summary>Dapper-backed repository for Compliance.FMCSAStandards read-model queries.</summary>
public sealed class FmcsaStandardsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<FmcsaStandardsDapperRepository> logger)
    : IFmcsaStandardsRepository
{
    /// <inheritdoc/>
    public async Task<FmcsaStandardDto?> GetStandardBySectionAsync(string partNumber, string sectionNumber, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetStandardBySection for FMCSAStandards Part {PartNumber} Section {SectionNumber}", partNumber, sectionNumber);

        var parameters = new StoredProcedureBuilder()
            .AddString("PartNumber", partNumber, 10)
            .AddString("SectionNumber", sectionNumber, 50)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<StandardRow>(
            "[Compliance].[usp_FmcsaStandards_GetBySection]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaStandardDto>> SearchStandardsAsync(
        string? keyword, string? partNumber, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 200)
            .AddString("PartNumber", partNumber, 10)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<StandardRow>(
            "[Compliance].[usp_FmcsaStandards_Search]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaRequirementDto>> GetRequirementsForStandardAsync(Guid standardId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("StandardId", standardId).Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_FmcsaRequirements_GetByStandardId]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaRequirementDto>> GetRequirementsByModuleAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddString("AppliesToModule", appliesToModule, 100).Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_FmcsaRequirements_GetByModule]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaRequirementDto>> SearchRequirementsAsync(string keyword, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 400)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_FmcsaRequirements_Search]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaStandardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddDateTime("SinceUtc", sinceUtc).Build();

        var rows = await queryExecutor.QueryAsync<ChangeRow>(
            "[Compliance].[usp_FmcsaStandardChangeLog_GetSince]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static FmcsaStandardDto ToDto(StandardRow row) => new()
    {
        StandardId = row.StandardId,
        PartNumber = row.PartNumber,
        SubpartLetter = row.SubpartLetter,
        SectionNumber = row.SectionNumber,
        SectionTitle = row.SectionTitle,
        SectionText = row.SectionText,
        SectionTextHash = row.SectionTextHash,
        SourceUrl = row.SourceUrl,
        CreatedOn = row.CreatedOn,
    };

    private static FmcsaRequirementDto ToDto(RequirementRow row) => new()
    {
        RequirementId = row.RequirementId,
        StandardId = row.StandardId,
        RequirementNumber = row.RequirementNumber,
        RequirementText = row.RequirementText,
        Severity = row.Severity,
        AppliesToModule = row.AppliesToModule,
        Rationale = row.Rationale,
    };

    private static FmcsaStandardChangeDto ToDto(ChangeRow row) => new()
    {
        ChangeLogId = row.ChangeLogId,
        StandardId = row.StandardId,
        PartNumber = row.PartNumber,
        SectionNumber = row.SectionNumber,
        ChangeType = row.ChangeType,
        PreviousTextHash = row.PreviousTextHash,
        NewTextHash = row.NewTextHash,
        DetectedAtUtc = row.DetectedAtUtc,
        Summary = row.Summary,
    };

    private sealed record StandardRow
    {
        public required Guid StandardId { get; init; }
        public required string PartNumber { get; init; }
        public string? SubpartLetter { get; init; }
        public required string SectionNumber { get; init; }
        public string? SectionTitle { get; init; }
        public required string SectionText { get; init; }
        public string? SectionTextHash { get; init; }
        public string? SourceUrl { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record RequirementRow
    {
        public required Guid RequirementId { get; init; }
        public required Guid StandardId { get; init; }
        public string? RequirementNumber { get; init; }
        public required string RequirementText { get; init; }
        public string? Severity { get; init; }
        public string? AppliesToModule { get; init; }
        public string? Rationale { get; init; }
    }

    private sealed record ChangeRow
    {
        public required Guid ChangeLogId { get; init; }
        public required Guid StandardId { get; init; }
        public required string PartNumber { get; init; }
        public required string SectionNumber { get; init; }
        public required string ChangeType { get; init; }
        public string? PreviousTextHash { get; init; }
        public string? NewTextHash { get; init; }
        public required DateTime DetectedAtUtc { get; init; }
        public string? Summary { get; init; }
    }
}
