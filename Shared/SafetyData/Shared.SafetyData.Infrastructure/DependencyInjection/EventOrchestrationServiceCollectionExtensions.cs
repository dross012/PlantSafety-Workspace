// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           EventOrchestrationServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the interim runtime event
//                  orchestration layer. Registers ITenantFacilityRegistry
//                  (singleton -- must be the same instance every request
//                  registers against and every event handler resolves
//                  against) and IEventOrchestrationService (singleton --
//                  stateless, delegates to the other already-singleton
//                  domain-core services). Does not itself subscribe to
//                  Shared.EventBus -- the actual IEventHandler<T>
//                  registrations against real integration events
//                  (AccidentCreatedIntegrationEvent, etc.) are a
//                  Reporting.Infrastructure-level concern (see
//                  Reporting.Infrastructure\EventBus\DependencyInjection\
//                  SafetyOrchestrationServiceCollectionExtensions.cs), since
//                  that project already owns the Shared.EventBus SQL
//                  transport and hosted-service polling this depends on.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the interim runtime event orchestration layer's services with the DI container.</summary>
public static class EventOrchestrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ITenantFacilityRegistry"/> (backed by <see cref="TenantFacilityRegistry"/>)
    /// and <see cref="IEventOrchestrationService"/> (backed by <see cref="EventOrchestrationService"/>)
    /// as singletons.
    /// </summary>
    public static IServiceCollection AddEventBusOrchestration(this IServiceCollection services)
    {
        services.AddSingleton<ITenantFacilityRegistry, TenantFacilityRegistry>();
        services.AddSingleton<IEventOrchestrationService, EventOrchestrationService>();

        return services;
    }
}
