// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           FacilityProfileBuilder.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic builder that computes a FacilityProfile from
//                  an industry name plus caller-supplied facility identity/
//                  location/completion data. Every derived field is computed
//                  from the existing Industry, Regulatory, and Safety
//                  Programs subsystems (IndustryRegistry.All, HazardMapping's
//                  and RequiredSafetyModules' underlying SafetyFlagCatalog
//                  decomposition via industry.Flags, RegulatorRequirement
//                  Mapping's underlying regulator list via industry.Regulators,
//                  and ProgramRequirementMapping.For) -- no hand-authored
//                  per-facility content. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>Builds <see cref="FacilityProfile"/> instances deterministically from an industry binding.</summary>
public static class FacilityProfileBuilder
{
    /// <summary>
    /// Builds the full <see cref="FacilityProfile"/> for a facility bound to
    /// <paramref name="industryName"/>.
    /// </summary>
    /// <param name="facilityId">Caller-supplied facility identifier (see FacilityProfile.cs's SCOPE NOTE).</param>
    /// <param name="name">Caller-supplied facility display name.</param>
    /// <param name="industryName">The exact, canonical name of an industry in <see cref="IndustryRegistry.All"/>.</param>
    /// <param name="location">Caller-supplied facility location (free text).</param>
    /// <param name="completedElementCodes">
    /// The set of <see cref="ComplianceElement.Code"/> values this facility has actually
    /// completed, as reported by the caller. Defaults to empty (a freshly onboarded facility
    /// with nothing yet completed) when omitted.
    /// </param>
    /// <exception cref="ArgumentException">No industry named <paramref name="industryName"/> exists in <see cref="IndustryRegistry.All"/>.</exception>
    public static FacilityProfile Build(
        Guid facilityId,
        string name,
        string industryName,
        string location,
        IReadOnlySet<string>? completedElementCodes = null)
    {
        IndustryDefinition industry = IndustryRegistry.FindByName(industryName)
            ?? throw new ArgumentException($"No industry named '{industryName}' exists in IndustryRegistry.All.", nameof(industryName));

        IReadOnlyList<ProgramDefinition> requiredPrograms = ProgramRequirementMapping.For(industry);

        IReadOnlyList<PermitType> requiredPermits = requiredPrograms
            .Select(ProgramPermitRequirements.For)
            .Distinct()
            .ToArray();

        IReadOnlyList<TrainingRequirement> requiredTraining = requiredPrograms
            .Select(ProgramTrainingRequirements.For)
            .ToArray();

        IReadOnlyList<AuditChecklistItem> requiredAudits = requiredPrograms
            .Select(ProgramAuditRequirements.For)
            .ToArray();

        IReadOnlyList<SopTemplate> requiredSops = requiredPrograms
            .Select(ProgramSopRequirements.For)
            .ToArray();

        IReadOnlyList<ComplianceElement> requiredElements = FacilityComplianceProfileBuilder.BuildRequiredElements(requiredPrograms);
        FacilityComplianceProfile complianceProfile = FacilityComplianceProfileBuilder.Build(
            requiredElements,
            completedElementCodes ?? new HashSet<string>(StringComparer.Ordinal));

        return new FacilityProfile(
            FacilityId: facilityId,
            Name: name,
            IndustryName: industry.Name,
            Location: location,
            Flags: industry.Flags,
            Regulators: industry.Regulators,
            RequiredPrograms: requiredPrograms,
            RequiredPermits: requiredPermits,
            RequiredTraining: requiredTraining,
            RequiredAudits: requiredAudits,
            RequiredSops: requiredSops,
            ComplianceProfile: complianceProfile);
    }
}
