// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           FacilityProfileService.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-resolvable IFacilityProfileService implementation that
//                  delegates every member to the static
//                  FacilityProfileBuilder/FacilityDashboardMetadataBuilder/
//                  FacilityEventRoutingRulesBuilder/FacilityOnboardingTemplateBuilder
//                  (Abstractions.Models.Industries.Facilities). Holds no state
//                  of its own -- mirrors IndustryRegistryProvider,
//                  RegulatoryRequirementRegistryProvider, and
//                  ProgramRegistryProvider exactly. Stateless, so a single
//                  instance is safe to share as a singleton.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>Stateless <see cref="IFacilityProfileService"/> implementation backed by the static Facility Profile builders.</summary>
public sealed class FacilityProfileService : IFacilityProfileService
{
    /// <inheritdoc />
    public FacilityProfile BuildProfile(
        Guid facilityId,
        string name,
        string industryName,
        string location,
        IReadOnlySet<string>? completedElementCodes = null) =>
        FacilityProfileBuilder.Build(facilityId, name, industryName, location, completedElementCodes);

    /// <inheritdoc />
    public FacilityDashboardMetadata BuildDashboardMetadata(FacilityProfile facility) =>
        FacilityDashboardMetadataBuilder.Build(facility);

    /// <inheritdoc />
    public IReadOnlyList<FacilityEventRoute> BuildEventRoutingRules(FacilityProfile facility) =>
        FacilityEventRoutingRulesBuilder.Build(facility);

    /// <inheritdoc />
    public FacilityOnboardingTemplate? GetOnboardingTemplate(string industryName) =>
        FacilityOnboardingTemplateBuilder.ByIndustryName.TryGetValue(industryName, out FacilityOnboardingTemplate? template)
            ? template
            : null;
}
