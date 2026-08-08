// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards
//  Layer:          Infrastructure
//  File:           StandardsReferenceDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ANSI/ASME/ISO consensus-standards reference
//                  read-model (IStandardsReferenceRepository). Executes stored procedures for
//                  all persistence operations, mirroring EpaReferenceDapperRepository /
//                  OSHAStandardsDapperRepository's precedent. Global reference data -- these
//                  calls deliberately carry no TenantId parameter (there is no tenant to scope
//                  consensus-standards text to), so DapperQueryExecutor's optional
//                  TenantContextPropagation enforcement (currently OFF platform-wide, see
//                  TenantContextPropagationOptions) must never be turned on without excluding
//                  this repository's stored procedures. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Standards.Persistence;

/// <summary>Dapper-backed repository for Compliance.Standards reference-data read-model queries.</summary>
public sealed class StandardsReferenceDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<StandardsReferenceDapperRepository> logger)
    : IStandardsReferenceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<StandardsFamilyDto>> GetAllFamiliesAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAllFamilies for StandardsFamilies");

        var parameters = new StoredProcedureBuilder().Build();

        var rows = await queryExecutor.QueryAsync<FamilyRow>(
            "[Compliance].[usp_StandardsFamilies_GetAll]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StandardsSeriesDto>> GetSeriesByFamilyAsync(string familyCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSeriesByFamily for StandardsSeries FamilyCode {FamilyCode}", familyCode);

        var parameters = new StoredProcedureBuilder()
            .AddString("FamilyCode", familyCode, 10)
            .Build();

        var rows = await queryExecutor.QueryAsync<SeriesRow>(
            "[Compliance].[usp_StandardsSeries_GetByFamily]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StandardsRequirementDto>> GetRequirementsByFamilyAsync(
        string familyCode, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("FamilyCode", familyCode, 10)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Standards_GetByFamily]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StandardsRequirementDto>> GetRequirementsBySeriesAsync(
        string familyCode, string seriesCode, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("FamilyCode", familyCode, 10)
            .AddString("SeriesCode", seriesCode, 30)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Standards_GetBySeries]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StandardsRequirementDto>> GetRequirementsByModuleAsync(
        string moduleName, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("ModuleName", moduleName, 100)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Standards_GetByModule]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StandardsRequirementDto>> SearchRequirementsAsync(
        string? keyword, string? familyCode, int skip, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Keyword", keyword, 200)
            .AddString("FamilyCode", familyCode, 10)
            .AddInt("Skip", skip)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<RequirementRow>(
            "[Compliance].[usp_Standards_SearchRequirements]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<StandardsRequirementDto?> GetRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("RequirementId", requirementId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RequirementRow>(
            "[Compliance].[usp_Standards_GetRequirementById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EquipmentRequirementDto>> GetEquipmentRequirementsAsync(
        string? equipmentType, Guid? requirementId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("EquipmentType", equipmentType, 50)
            .AddGuid("RequirementId", requirementId)
            .Build();

        var rows = await queryExecutor.QueryAsync<EquipmentRequirementRow>(
            "[Compliance].[usp_Standards_GetEquipmentRequirements]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RiskAssessmentRuleDto>> GetRiskAssessmentRulesAsync(
        string? methodology, Guid? requirementId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddString("Methodology", methodology, 100)
            .AddGuid("RequirementId", requirementId)
            .Build();

        var rows = await queryExecutor.QueryAsync<RiskAssessmentRuleRow>(
            "[Compliance].[usp_Standards_GetRiskAssessmentRules]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static StandardsFamilyDto ToDto(FamilyRow row) => new()
    {
        FamilyId = row.FamilyId,
        FamilyCode = row.FamilyCode,
        FamilyName = row.FamilyName,
        Description = row.Description,
        CreatedOn = row.CreatedOn,
    };

    private static StandardsSeriesDto ToDto(SeriesRow row) => new()
    {
        SeriesId = row.SeriesId,
        FamilyCode = row.FamilyCode,
        SeriesCode = row.SeriesCode,
        SeriesName = row.SeriesName,
        Description = row.Description,
        CreatedOn = row.CreatedOn,
    };

    private static StandardsRequirementDto ToDto(RequirementRow row) => new()
    {
        RequirementId = row.RequirementId,
        FamilyCode = row.FamilyCode,
        SeriesCode = row.SeriesCode,
        Title = row.Title,
        Description = row.Description,
        ApplicabilityConditions = row.ApplicabilityConditions,
        ImpactedModules = row.ImpactedModules,
        Severity = row.Severity,
        SourceUrl = row.SourceUrl,
        CreatedOn = row.CreatedOn,
    };

    private static EquipmentRequirementDto ToDto(EquipmentRequirementRow row) => new()
    {
        EquipmentRequirementId = row.EquipmentRequirementId,
        RequirementId = row.RequirementId,
        EquipmentType = row.EquipmentType,
        RequirementText = row.RequirementText,
        InspectionIntervalDescription = row.InspectionIntervalDescription,
        ControlSystemCategory = row.ControlSystemCategory,
        CreatedOn = row.CreatedOn,
    };

    private static RiskAssessmentRuleDto ToDto(RiskAssessmentRuleRow row) => new()
    {
        RiskAssessmentRuleId = row.RiskAssessmentRuleId,
        RequirementId = row.RequirementId,
        Methodology = row.Methodology,
        HazardIdentificationSteps = DeserializeStringList(row.HazardIdentificationSteps),
        RiskReductionMeasures = DeserializeStringList(row.RiskReductionMeasures),
        Description = row.Description,
        CreatedOn = row.CreatedOn,
    };

    private static IReadOnlyList<string> DeserializeStringList(string json)
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

    private sealed record FamilyRow
    {
        public required Guid FamilyId { get; init; }
        public required string FamilyCode { get; init; }
        public required string FamilyName { get; init; }
        public string? Description { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record SeriesRow
    {
        public required Guid SeriesId { get; init; }
        public required string FamilyCode { get; init; }
        public required string SeriesCode { get; init; }
        public required string SeriesName { get; init; }
        public string? Description { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record RequirementRow
    {
        public required Guid RequirementId { get; init; }
        public required string FamilyCode { get; init; }
        public required string SeriesCode { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public string? ApplicabilityConditions { get; init; }
        public required string ImpactedModules { get; init; }
        public required string Severity { get; init; }
        public string? SourceUrl { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record EquipmentRequirementRow
    {
        public required Guid EquipmentRequirementId { get; init; }
        public required Guid RequirementId { get; init; }
        public required string EquipmentType { get; init; }
        public required string RequirementText { get; init; }
        public string? InspectionIntervalDescription { get; init; }
        public string? ControlSystemCategory { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    private sealed record RiskAssessmentRuleRow
    {
        public required Guid RiskAssessmentRuleId { get; init; }
        public required Guid RequirementId { get; init; }
        public required string Methodology { get; init; }
        public required string HazardIdentificationSteps { get; init; }
        public required string RiskReductionMeasures { get; init; }
        public string? Description { get; init; }
        public required DateTime CreatedOn { get; init; }
    }
}
