// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           IFacilityProfileService.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-friendly abstraction over FacilityProfileBuilder,
//                  FacilityDashboardMetadataBuilder, FacilityEventRoutingRulesBuilder,
//                  and FacilityOnboardingTemplateBuilder, so consumers can
//                  take a constructor dependency instead of static references
//                  -- mirrors IIndustryRegistry/IRegulatoryRequirementRegistry/
//                  IProgramRegistry's role for their own subsystems exactly.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>
/// DI-resolvable facade over the Facility Profile subsystem's deterministic builders.
/// </summary>
public interface IFacilityProfileService
{
    /// <summary>Builds a facility's full profile (see <see cref="FacilityProfileBuilder.Build"/>).</summary>
    /// <exception cref="ArgumentException">No industry named <paramref name="industryName"/> exists.</exception>
    FacilityProfile BuildProfile(
        Guid facilityId,
        string name,
        string industryName,
        string location,
        IReadOnlySet<string>? completedElementCodes = null);

    /// <summary>Builds a facility's dashboard metadata from an already-built <see cref="FacilityProfile"/>.</summary>
    FacilityDashboardMetadata BuildDashboardMetadata(FacilityProfile facility);

    /// <summary>Builds a facility's event routing rules from an already-built <see cref="FacilityProfile"/>.</summary>
    IReadOnlyList<FacilityEventRoute> BuildEventRoutingRules(FacilityProfile facility);

    /// <summary>
    /// Returns the onboarding template for the given industry, or <see langword="null"/> if no
    /// such industry exists in <see cref="IndustryRegistry.All"/>.
    /// </summary>
    FacilityOnboardingTemplate? GetOnboardingTemplate(string industryName);
}
