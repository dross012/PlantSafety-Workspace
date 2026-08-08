// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           ComplianceRiskServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the Compliance & Risk subsystem.
//                  Registers IComplianceRiskService -> ComplianceRiskService
//                  as a singleton; the service is stateless and delegates
//                  every call to the static ComplianceRiskEngine, so a
//                  single shared instance is safe. Mirrors
//                  EventingServiceCollectionExtensions.AddEventing()'s,
//                  FacilityProfileServiceCollectionExtensions.
//                  AddFacilityProfiles()'s, ProgramServiceCollectionExtensions.
//                  AddSafetyPrograms()'s, RegulatoryServiceCollectionExtensions.
//                  AddRegulatoryRequirements()'s, and
//                  IndustryServiceCollectionExtensions.AddIndustries()'s
//                  naming/shape exactly.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the Compliance &amp; Risk subsystem's services with the DI container.</summary>
public static class ComplianceRiskServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IComplianceRiskService"/> (backed by <see cref="ComplianceRiskService"/>)
    /// as a singleton over the static Compliance &amp; Risk subsystem engine.
    /// </summary>
    public static IServiceCollection AddComplianceRisk(this IServiceCollection services)
    {
        services.AddSingleton<IComplianceRiskService, ComplianceRiskService>();

        return services;
    }
}
