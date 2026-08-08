// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           FacilityOnboardingTemplate.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic onboarding template for a new facility in a
//                  given industry: the full onboarding checklist (every
//                  required element, before any of it has been completed --
//                  equivalent to FacilityComplianceProfileBuilder.
//                  BuildRequiredElements with an empty completed set), the
//                  programs to activate, and the permits to activate.
//                  Industry-scoped rather than facility-scoped, since a
//                  template describes what any new facility in that industry
//                  needs before one exists -- unlike FacilityProfile, which
//                  requires a caller-supplied facility identity. Computed
//                  entirely from IndustryRegistry.All and
//                  ProgramRequirementMapping -- no hand-authored per-industry
//                  content. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>
/// A deterministic onboarding template for a new facility in a given industry: the full
/// onboarding checklist, the programs to activate, and the permits to activate.
/// </summary>
/// <param name="IndustryName">The exact, canonical industry name this template was built for.</param>
/// <param name="OnboardingChecklist">Every required compliance element for this industry, none yet completed.</param>
/// <param name="ProgramActivationList">Every Safety Program a new facility in this industry must activate.</param>
/// <param name="PermitActivationList">Every distinct permit type a new facility in this industry must activate.</param>
public sealed record FacilityOnboardingTemplate(
    string IndustryName,
    IReadOnlyList<ComplianceElement> OnboardingChecklist,
    IReadOnlyList<ProgramDefinition> ProgramActivationList,
    IReadOnlyList<PermitType> PermitActivationList);

/// <summary>
/// Deterministic Industry -> Onboarding template mapping, computed from
/// <see cref="IndustryRegistry.All"/> and <see cref="ProgramRequirementMapping"/>.
/// </summary>
public static class FacilityOnboardingTemplateBuilder
{
    /// <summary>Builds the onboarding template for the given industry.</summary>
    public static FacilityOnboardingTemplate For(IndustryDefinition industry)
    {
        IReadOnlyList<ProgramDefinition> programs = ProgramRequirementMapping.For(industry);

        IReadOnlyList<PermitType> permits = programs
            .Select(ProgramPermitRequirements.For)
            .Distinct()
            .ToArray();

        IReadOnlyList<ComplianceElement> checklist = FacilityComplianceProfileBuilder.BuildRequiredElements(programs);

        return new FacilityOnboardingTemplate(industry.Name, checklist, programs, permits);
    }

    /// <summary>Onboarding templates for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, FacilityOnboardingTemplate> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(industry => industry.Name, For, StringComparer.Ordinal);
}
