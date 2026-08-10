// ============================================================================
//  Project:        Shared.EmployeeDomain.Infrastructure
//  Module:         DependencyInjection
//  File:           EmployeeDomainServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Description:    Registers Shared.EmployeeDomain's in-memory store,
//                  repository, and application service with the DI
//                  container. IEmployeeStore is a singleton (it IS the
//                  process-local store); IEmployeeRepository/IEmployeeService
//                  are scoped, matching Shared.FacilityDomain's convention.
//
//                  Phase 4 (Employee domain pass): EmployeeService now
//                  depends on Shared.Deterministic's IClock and
//                  Shared.ModuleContracts'/Shared.Telemetry's
//                  IModuleEventGateway/ILogEventWriter/ICorrelationContext.
//                  The caller's host must already have those registered
//                  (e.g. via AddDeterministicRuntime + the Phase 2.4/2.7
//                  telemetry/event-gateway registrations PlantSafety.UI.Api's
//                  Program.cs already does) before calling AddEmployeeDomain.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Infrastructure.Persistence;
using Shared.EmployeeDomain.Infrastructure.Services;

namespace Shared.EmployeeDomain.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.EmployeeDomain's services with the DI container.</summary>
public static class EmployeeDomainServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IEmployeeStore"/> (singleton), <see cref="IEmployeeRepository"/>, and <see cref="IEmployeeService"/> (both scoped).</summary>
    public static IServiceCollection AddEmployeeDomain(this IServiceCollection services)
    {
        services.AddSingleton<IEmployeeStore, EmployeeStore>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();

        return services;
    }
}
