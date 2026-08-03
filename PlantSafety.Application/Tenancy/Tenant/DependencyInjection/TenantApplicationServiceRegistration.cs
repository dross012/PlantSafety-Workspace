// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Registers Tenant Application-layer services (the Domain-layer factory).
//   MediatR command/query handlers are discovered automatically by the platform's existing
//   MediatR assembly-scanning registration; only the factory needs explicit DI wiring here.
//   Self-contained extension method; wired into the composition root by the host application.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Domain.Tenancy.Tenant.Factories;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;

namespace PlantSafety.Application.Tenancy.Tenant.DependencyInjection;

/// <summary>Extension methods to register Tenant Application-layer services.</summary>
public static class TenantApplicationServiceRegistration
{
    /// <summary>Registers Tenant-specific Application services.</summary>
    public static IServiceCollection AddTenantApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITenantFactory, TenantFactory>();

        return services;
    }
}
