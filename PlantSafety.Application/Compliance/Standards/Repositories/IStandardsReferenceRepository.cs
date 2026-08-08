// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the ANSI/ASME/ISO
//   consensus-standards reference data (Compliance.StandardsFamily/StandardsSeries/
//   StandardsRequirement/StandardsEquipmentRequirement/StandardsRiskAssessmentRule). Global
//   data -- every method is deliberately un-tenant-scoped, mirroring
//   IEpaReferenceRepository/IOSHAStandardsRepository's precedent. Implemented in
//   Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Repositories;

/// <summary>Read-only repository for ANSI/ASME/ISO consensus-standards reference data. Implemented in Infrastructure.</summary>
public interface IStandardsReferenceRepository
{
    /// <summary>Returns every standards family row (ANSI/ASME/ISO), ordered by FamilyCode.</summary>
    Task<IReadOnlyList<StandardsFamilyDto>> GetAllFamiliesAsync(CancellationToken cancellationToken);

    /// <summary>Returns every standards series row for a given family, ordered by SeriesCode.</summary>
    Task<IReadOnlyList<StandardsSeriesDto>> GetSeriesByFamilyAsync(string familyCode, CancellationToken cancellationToken);

    /// <summary>Returns every standards requirement for a given family, paged, ordered by SeriesCode/Title.</summary>
    Task<IReadOnlyList<StandardsRequirementDto>> GetRequirementsByFamilyAsync(
        string familyCode, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns every standards requirement for a given (family, series), paged, ordered by Title.</summary>
    Task<IReadOnlyList<StandardsRequirementDto>> GetRequirementsBySeriesAsync(
        string familyCode, string seriesCode, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns every standards requirement whose ImpactedModules field contains a given PlantSafety module, paged.</summary>
    Task<IReadOnlyList<StandardsRequirementDto>> GetRequirementsByModuleAsync(
        string moduleName, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Free-text/family search across Compliance.StandardsRequirement, paged.</summary>
    Task<IReadOnlyList<StandardsRequirementDto>> SearchRequirementsAsync(
        string? keyword, string? familyCode, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns a single standards requirement by its identity, or null.</summary>
    Task<StandardsRequirementDto?> GetRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken);

    /// <summary>Returns equipment requirements, optionally filtered by equipment type and/or parent requirement.</summary>
    Task<IReadOnlyList<EquipmentRequirementDto>> GetEquipmentRequirementsAsync(
        string? equipmentType, Guid? requirementId, CancellationToken cancellationToken);

    /// <summary>Returns risk assessment rules, optionally filtered by methodology and/or parent requirement.</summary>
    Task<IReadOnlyList<RiskAssessmentRuleDto>> GetRiskAssessmentRulesAsync(
        string? methodology, Guid? requirementId, CancellationToken cancellationToken);
}
