// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the EPA
//   regulatory reference data (Compliance.EpaProgram/EpaRegulatoryRequirement/
//   EpaThresholdRule/EpaReportingRequirement/EpaRecordkeepingRequirement). Global data --
//   every method is deliberately un-tenant-scoped, mirroring IOSHAStandardsRepository's
//   precedent. Implemented in Infrastructure. Uses the "Epa" prefix (not bare "EPA") to
//   avoid collision with the pre-existing, unrelated Compliance.Certification.EPA /
//   Compliance.RegulatoryCalendar.Deadlines.EPA features.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Repositories;

/// <summary>Read-only repository for EPA regulatory reference data. Implemented in Infrastructure.</summary>
public interface IEpaReferenceRepository
{
    /// <summary>Returns every EPA program row.</summary>
    Task<IReadOnlyList<EpaProgramDto>> GetAllProgramsAsync(CancellationToken cancellationToken);

    /// <summary>Returns a single EPA program by its short code, or null.</summary>
    Task<EpaProgramDto?> GetProgramByCodeAsync(string programCode, CancellationToken cancellationToken);

    /// <summary>Returns every regulatory requirement for a given EPA program, paged, ordered by Citation.</summary>
    Task<IReadOnlyList<EpaRegulatoryRequirementDto>> GetRequirementsByProgramAsync(
        string programCode, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns every regulatory requirement whose ImpactedModules field contains a given PlantSafety module, paged.</summary>
    Task<IReadOnlyList<EpaRegulatoryRequirementDto>> GetRequirementsByModuleAsync(
        string moduleName, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Free-text/program search across Compliance.EpaRegulatoryRequirement, paged.</summary>
    Task<IReadOnlyList<EpaRegulatoryRequirementDto>> SearchRequirementsAsync(
        string? keyword, string? programCode, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns a single regulatory requirement by its identity, or null.</summary>
    Task<EpaRegulatoryRequirementDto?> GetRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken);

    /// <summary>Returns numeric threshold rules, optionally filtered by program and/or parameter name.</summary>
    Task<IReadOnlyList<EpaThresholdRuleDto>> GetThresholdRulesAsync(
        string? programCode, string? parameterName, CancellationToken cancellationToken);

    /// <summary>Returns reporting requirements, optionally filtered by program.</summary>
    Task<IReadOnlyList<EpaReportingRequirementDto>> GetReportingRequirementsAsync(
        string? programCode, CancellationToken cancellationToken);

    /// <summary>Returns recordkeeping requirements, optionally filtered by program.</summary>
    Task<IReadOnlyList<EpaRecordkeepingRequirementDto>> GetRecordkeepingRequirementsAsync(
        string? programCode, CancellationToken cancellationToken);
}
