// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           FacilityProfileServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the Facility Profile subsystem.
//                  Registers IFacilityProfileService -> FacilityProfileService
//                  as a singleton; the service is stateless and delegates
//                  every call to the static Facility Profile builders, so a
//                  single shared instance is safe. Mirrors
//                  ProgramServiceCollectionExtensions.AddSafetyPrograms()'s,
//                  RegulatoryServiceCollectionExtensions.
//                  AddRegulatoryRequirements()'s, and
//                  IndustryServiceCollectionExtensions.AddIndustries()'s
//                  naming/shape exactly.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the Facility Profile subsystem's services with the DI container.</summary>
public static class FacilityProfileServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IFacilityProfileService"/> (backed by <see cref="FacilityProfileService"/>)
    /// as a singleton over the static Facility Profile subsystem builders.
    /// </summary>
    public static IServiceCollection AddFacilityProfiles(this IServiceCollection services)
    {
        services.AddSingleton<IFacilityProfileService, FacilityProfileService>();

        return services;
    }
}
