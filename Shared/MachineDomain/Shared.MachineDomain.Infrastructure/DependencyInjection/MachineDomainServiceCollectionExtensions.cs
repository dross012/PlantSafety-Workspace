// ============================================================================
//  Project:        Shared.MachineDomain.Infrastructure
//  Module:         DependencyInjection
//  File:           MachineDomainServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Description:    Registers Shared.MachineDomain's in-memory store,
//                  repository, and application service with the DI
//                  container. IMachineStore is a singleton (it IS the
//                  process-local store); IMachineRepository/IMachineService
//                  are scoped, matching Shared.FacilityDomain/Shared.
//                  EmployeeDomain's convention.
//
//                  Phase 4 (Machine domain pass): MachineService now
//                  depends on Shared.Deterministic's IClock and
//                  Shared.ModuleContracts'/Shared.Telemetry's
//                  IModuleEventGateway/ILogEventWriter/ICorrelationContext.
//                  The caller's host must already have those registered
//                  before calling AddMachineDomain.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Infrastructure.Persistence;
using Shared.MachineDomain.Infrastructure.Services;

namespace Shared.MachineDomain.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.MachineDomain's services with the DI container.</summary>
public static class MachineDomainServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IMachineStore"/> (singleton), <see cref="IMachineRepository"/>, and <see cref="IMachineService"/> (both scoped).</summary>
    public static IServiceCollection AddMachineDomain(this IServiceCollection services)
    {
        services.AddSingleton<IMachineStore, MachineStore>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IMachineService, MachineService>();

        return services;
    }
}
