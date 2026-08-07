// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.OSHAStandards
//  Layer:          Infrastructure
//  File:           OSHAStandardsDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the OSHA standards read-model
//                  (IOSHAStandardsRepository). Executes stored procedures for all persistence
//                  operations, mirroring OSHACertificationsDapperRepository/
//                  ComplianceEvaluationReadDapperRepository's precedent. Global reference data
//                  -- these calls deliberately carry no TenantId parameter (there is no tenant
//                  to scope OSHA regulatory text to), so DapperQueryExecutor's optional
//                  TenantContextPropagation enforcement (currently OFF platform-wide, see
//                  TenantContextPropagationOptions) must never be turned on without excluding
//                  this repository's stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Infrastructure.Compliance.OSHAStandards.Persistence;

/// <summary>Dapper-backed repository for Compliance.OSHAStandards read-model queries.</summary>
public sealed class OSHAStandardsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<OSHAStandardsDapperRepository> logger)
    : IOSHAStandardsRepository
{
    /// <inheritdoc/>
    public async Task<OSHAStandardItemDto?> GetBySectionAsync(int partNumber, string section, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetBySection for OSHAStandards Part {PartNumber} Section {Section}", partNumber, section);

        var parameters = new StoredProcedureBuilder()
            .AddInt("PartNumber", partNumber)
            .AddString("Section", section, 50)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<StandardRow>(
            "[Compliance].[usp_OSHAStandards_GetBySection]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHAStandardItemDto>> SearchAsync(
        string? keyword, int? partNumber, string? status, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 400)
            .AddInt("PartNumber", partNumber)
            .AddString("Status", status, 50)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<StandardRow>(
            "[Compliance].[usp_OSHAStandards_Search]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHARequirementItemDto>> GetRequirementsForStandardAsync(Guid oshaStandardId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("OSHAStandardId", oshaStandardId).Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_OSHARequirements_GetByStandardId]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHARequirementItemDto>> GetRequirementsByModuleAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddString("AppliesToModule", appliesToModule, 100).Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_OSHARequirements_GetByModule]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHARequirementItemDto>> SearchRequirementsAsync(string keyword, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 400)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_OSHARequirements_Search]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHAStandardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddDateTime("SinceUtc", sinceUtc).Build();

        var rows = await queryExecutor.QueryAsync<ChangeRow>(
            "[Compliance].[usp_OSHAStandardChangeLog_GetSince]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static OSHAStandardItemDto ToDto(StandardRow row) => new()
    {
        OSHAStandardId = row.OSHAStandardId,
        TitleNumber = row.TitleNumber,
        PartNumber = row.PartNumber,
        PartTitle = row.PartTitle,
        Subpart = row.Subpart,
        SubpartTitle = row.SubpartTitle,
        Section = row.Section,
        SectionTitle = row.SectionTitle,
        SectionText = row.SectionText,
        EffectiveDate = row.EffectiveDate.HasValue ? DateOnly.FromDateTime(row.EffectiveDate.Value) : null,
        Status = row.Status,
        SourceUrl = row.SourceUrl,
        CreatedAtUtc = row.CreatedAtUtc,
        UpdatedAtUtc = row.UpdatedAtUtc,
    };

    private static OSHARequirementItemDto ToDto(RequirementRow row) => new()
    {
        OSHARequirementId = row.OSHARequirementId,
        OSHAStandardId = row.OSHAStandardId,
        RequirementType = row.RequirementType,
        AppliesToModule = row.AppliesToModule,
        Severity = row.Severity,
        Notes = row.Notes,
        CreatedAtUtc = row.CreatedAtUtc,
    };

    private static OSHAStandardChangeDto ToDto(ChangeRow row) => new()
    {
        ChangeLogId = row.ChangeLogId,
        OSHAStandardId = row.OSHAStandardId,
        PartNumber = row.PartNumber,
        Section = row.Section,
        ChangeType = row.ChangeType,
        PreviousStatus = row.PreviousStatus,
        NewStatus = row.NewStatus,
        DetectedAtUtc = row.DetectedAtUtc,
        Summary = row.Summary,
    };

    private sealed record StandardRow
    {
        public required Guid OSHAStandardId { get; init; }
        public required int TitleNumber { get; init; }
        public required int PartNumber { get; init; }
        public string? PartTitle { get; init; }
        public string? Subpart { get; init; }
        public string? SubpartTitle { get; init; }
        public required string Section { get; init; }
        public string? SectionTitle { get; init; }
        public required string SectionText { get; init; }
        public DateTime? EffectiveDate { get; init; }
        public required string Status { get; init; }
        public string? SourceUrl { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }

    private sealed record RequirementRow
    {
        public required Guid OSHARequirementId { get; init; }
        public required Guid OSHAStandardId { get; init; }
        public string? RequirementType { get; init; }
        public string? AppliesToModule { get; init; }
        public string? Severity { get; init; }
        public required string Notes { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
    }

    private sealed record ChangeRow
    {
        public required Guid ChangeLogId { get; init; }
        public required Guid OSHAStandardId { get; init; }
        public required int PartNumber { get; init; }
        public required string Section { get; init; }
        public required string ChangeType { get; init; }
        public string? PreviousStatus { get; init; }
        public string? NewStatus { get; init; }
        public required DateTime DetectedAtUtc { get; init; }
        public string? Summary { get; init; }
    }
}
