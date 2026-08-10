// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         DependencyInjection
//  File:           FacilityDomainServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Description:    Registers Shared.FacilityDomain's in-memory store,
//                  repository, and application service with the DI
//                  container. IFacilityStore is a singleton (it IS the
//                  process-local store); IFacilityRepository/IFacilityService
//                  are scoped, matching this codebase's established
//                  convention for stateless services over a singleton store.
//
//                  Phase 4 (Facility domain pass): FacilityService now
//                  depends on Shared.Deterministic's IClock and
//                  Shared.ModuleContracts'/Shared.Telemetry's
//                  IModuleEventGateway/ILogEventWriter/ICorrelationContext.
//                  The caller's host must already have those registered
//                  (e.g. via AddDeterministicRuntime + the Phase 2.4/2.7
//                  telemetry/event-gateway registrations PlantSafety.UI.Api's
//                  Program.cs already does) before calling AddFacilityDomain.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Infrastructure.Persistence;
using Shared.FacilityDomain.Infrastructure.Services;

namespace Shared.FacilityDomain.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.FacilityDomain's services with the DI container.</summary>
public static class FacilityDomainServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IFacilityStore"/> (singleton), <see cref="IFacilityRepository"/>, and <see cref="IFacilityService"/> (both scoped).</summary>
    public static IServiceCollection AddFacilityDomain(this IServiceCollection services)
    {
        services.AddSingleton<IFacilityStore, FacilityStore>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IFacilityService, FacilityService>();

        return services;
    }
}
