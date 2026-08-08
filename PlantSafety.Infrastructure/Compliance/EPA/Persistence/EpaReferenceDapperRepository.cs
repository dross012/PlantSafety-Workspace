// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA
//  Layer:          Infrastructure
//  File:           EpaReferenceDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EPA regulatory reference read-model
//                  (IEpaReferenceRepository). Executes stored procedures for all persistence
//                  operations, mirroring OSHAStandardsDapperRepository's precedent. Global
//                  reference data -- these calls deliberately carry no TenantId parameter
//                  (there is no tenant to scope EPA regulatory text to), so
//                  DapperQueryExecutor's optional TenantContextPropagation enforcement
//                  (currently OFF platform-wide, see TenantContextPropagationOptions) must
//                  never be turned on without excluding this repository's stored procedures.
//                  Uses the "Epa" prefix (not bare "EPA") to avoid collision with the
//                  pre-existing, unrelated Compliance.Certification.EPA /
//                  Compliance.RegulatoryCalendar.Deadlines.EPA features. Additive-only
//                  evolution.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Infrastructure.Compliance.EPA.Persistence;

/// <summary>Dapper-backed repository for Compliance.EPA reference-data read-model queries.</summary>
public sealed class EpaReferenceDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EpaReferenceDapperRepository> logger)
    : IEpaReferenceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaProgramDto>> GetAllProgramsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAllPrograms for EpaPrograms");

        var parameters = new StoredProcedureBuilder().Build();

        var rows = await queryExecutor.QueryAsync<ProgramRow>(
            "[Compliance].[usp_EpaPrograms_GetAll]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<EpaProgramDto?> GetProgramByCodeAsync(string programCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetProgramByCode for EpaPrograms ProgramCode {ProgramCode}", programCode);

        var parameters = new StoredProcedureBuilder()
            .AddString("ProgramCode", programCode, 20)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<ProgramRow>(
            "[Compliance].[usp_EpaPrograms_GetByCode]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaRegulatoryRequirementDto>> GetRequirementsByProgramAsync(
        string programCode, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("ProgramCode", programCode, 20)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Epa_GetRequirementsByProgram]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaRegulatoryRequirementDto>> GetRequirementsByModuleAsync(
        string moduleName, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("ModuleName", moduleName, 100)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Epa_GetRequirementsByModule]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaRegulatoryRequirementDto>> SearchRequirementsAsync(
        string? keyword, string? programCode, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 400)
            .AddString("ProgramCode", programCode, 20)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Epa_SearchRequirements]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<EpaRegulatoryRequirementDto?> GetRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("RequirementId", requirementId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RequirementRow>(
            "[Compliance].[usp_Epa_GetRequirementById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaThresholdRuleDto>> GetThresholdRulesAsync(
        string? programCode, string? parameterName, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("ProgramCode", programCode, 20)
            .AddString("ParameterName", parameterName, 200)
            .Build();

        var rows = await queryExecutor.QueryAsync<ThresholdRuleRow>(
            "[Compliance].[usp_Epa_GetThresholdRules]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaReportingRequirementDto>> GetReportingRequirementsAsync(
        string? programCode, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("ProgramCode", programCode, 20)
            .Build();

        var rows = await queryExecutor.QueryAsync<ReportingRequirementRow>(
            "[Compliance].[usp_Epa_GetReportingRequirements]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EpaRecordkeepingRequirementDto>> GetRecordkeepingRequirementsAsync(
        string? programCode, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("ProgramCode", programCode, 20)
            .Build();

        var rows = await queryExecutor.QueryAsync<RecordkeepingRequirementRow>(
            "[Compliance].[usp_Epa_GetRecordkeepingRequirements]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static EpaProgramDto ToDto(ProgramRow row) => new()
    {
        ProgramId = row.ProgramId,
        ProgramCode = row.ProgramCode,
        ProgramName = row.ProgramName,
        Description = row.Description,
        PrimaryStatuteCitation = row.PrimaryStatuteCitation,
        CreatedOn = row.CreatedOn,
    };

    private static EpaRegulatoryRequirementDto ToDto(RequirementRow row) => new()
    {
        RequirementId = row.RequirementId,
        ProgramCode = row.ProgramCode,
        Title = row.Title,
        Citation = row.Citation,
        Description = row.Description,
        ApplicabilityConditions = row.ApplicabilityConditions,
        ImpactedModules = row.ImpactedModules,
        Severity = row.Severity,
        SourceUrl = row.SourceUrl,
        CreatedOn = row.CreatedOn,
    };

    private static EpaThresholdRuleDto ToDto(ThresholdRuleRow row) => new()
    {
        ThresholdRuleId = row.ThresholdRuleId,
        ProgramCode = row.ProgramCode,
        RequirementId = row.RequirementId,
        ParameterName = row.ParameterName,
        LimitValue = row.LimitValue,
        Unit = row.Unit,
        ConditionExpression = row.ConditionExpression,
        CreatedOn = row.CreatedOn,
    };

    private static EpaReportingRequirementDto ToDto(ReportingRequirementRow row) => new()
    {
        ReportingRequirementId = row.ReportingRequirementId,
        ProgramCode = row.ProgramCode,
        RequirementId = row.RequirementId,
        TriggerDescription = row.TriggerDescription,
        DeadlineDescription = row.DeadlineDescription,
        ReportingDestination = row.ReportingDestination,
        FormName = row.FormName,
        CreatedOn = row.CreatedOn,
    };

    private static EpaRecordkeepingRequirementDto ToDto(RecordkeepingRequirementRow row) => new()
    {
        RecordkeepingRequirementId = row.RecordkeepingRequirementId,
        ProgramCode = row.ProgramCode,
        RequirementId = row.RequirementId,
        RetentionPeriodYears = row.RetentionPeriodYears,
        RequiredDataElements = DeserializeDataElements(row.RequiredDataElements),
        Description = row.Description,
        CreatedOn = row.CreatedOn,
    };

    private static IReadOnlyList<string> DeserializeDataElements(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record ProgramRow
    {
        public required Guid ProgramId { get; init; }
        public required string ProgramCode { get; init; }
        public required string ProgramName { get; init; }
        public string? Description { get; init; }
        public string? PrimaryStatuteCitation { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record RequirementRow
    {
        public required Guid RequirementId { get; init; }
        public required string ProgramCode { get; init; }
        public required string Title { get; init; }
        public required string Citation { get; init; }
        public required string Description { get; init; }
        public string? ApplicabilityConditions { get; init; }
        public required string ImpactedModules { get; init; }
        public required string Severity { get; init; }
        public string? SourceUrl { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record ThresholdRuleRow
    {
        public required Guid ThresholdRuleId { get; init; }
        public required string ProgramCode { get; init; }
        public Guid? RequirementId { get; init; }
        public required string ParameterName { get; init; }
        public required decimal LimitValue { get; init; }
        public required string Unit { get; init; }
        public string? ConditionExpression { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record ReportingRequirementRow
    {
        public required Guid ReportingRequirementId { get; init; }
        public required string ProgramCode { get; init; }
        public Guid? RequirementId { get; init; }
        public required string TriggerDescription { get; init; }
        public required string DeadlineDescription { get; init; }
        public required string ReportingDestination { get; init; }
        public string? FormName { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record RecordkeepingRequirementRow
    {
        public required Guid RecordkeepingRequirementId { get; init; }
        public required string ProgramCode { get; init; }
        public Guid? RequirementId { get; init; }
        public required int RetentionPeriodYears { get; init; }
        public required string RequiredDataElements { get; init; }
        public string? Description { get; init; }
        public required DateTime CreatedOn { get; init; }
    }
}
