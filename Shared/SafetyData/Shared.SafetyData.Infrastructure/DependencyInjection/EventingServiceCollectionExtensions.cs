// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           EventingServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the Eventing & Orchestration
//                  subsystem. Registers IEventRoutingService,
//                  IEventImpactService, and IEventWorkflowService as
//                  singletons; all three are stateless and delegate every
//                  call to the static Eventing catalogs/builders, so a
//                  single shared instance of each is safe. Mirrors
//                  FacilityProfileServiceCollectionExtensions.
//                  AddFacilityProfiles()'s, ProgramServiceCollectionExtensions.
//                  AddSafetyPrograms()'s, RegulatoryServiceCollectionExtensions.
//                  AddRegulatoryRequirements()'s, and
//                  IndustryServiceCollectionExtensions.AddIndustries()'s
//                  naming/shape exactly.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the Eventing &amp; Orchestration subsystem's services with the DI container.</summary>
public static class EventingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IEventRoutingService"/>, <see cref="IEventImpactService"/>, and
    /// <see cref="IEventWorkflowService"/> (backed by <see cref="EventRoutingService"/>,
    /// <see cref="EventImpactService"/>, and <see cref="EventWorkflowService"/> respectively) as
    /// singletons over the static Eventing &amp; Orchestration subsystem catalogs/builders.
    /// </summary>
    public static IServiceCollection AddEventing(this IServiceCollection services)
    {
        services.AddSingleton<IEventRoutingService, EventRoutingService>();
        services.AddSingleton<IEventImpactService, EventImpactService>();
        services.AddSingleton<IEventWorkflowService, EventWorkflowService>();

        return services;
    }
}
