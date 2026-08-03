// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Tenant Infrastructure-layer
//                  repositories and provisioning-pipeline services. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish (matching the AuditReport
//                  submodule's established precedent). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Infrastructure.Tenancy.Tenant.Persistence;
using PlantSafety.Infrastructure.Tenancy.Tenant.Provisioning;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Tenant submodule.</summary>
public static class TenantInfrastructureServiceRegistration
{
    /// <summary>Adds Tenant Dapper repositories and the automated provisioning-pipeline services.</summary>
    public static IServiceCollection AddTenantInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantDapperRepository>();
        services.AddScoped<ITenantReadRepository, TenantReadDapperRepository>();

        services.AddScoped<ITenantAdminUserProvisioner, TenantAdminUserProvisioner>();
        services.AddScoped<ITenantRoleAssigner, TenantRoleAssigner>();
        services.AddScoped<ITenantApiKeyGenerator, TenantApiKeyGenerator>();
        services.AddScoped<ITenantDataExportStore, TenantDataExportStore>();

        return services;
    }
}
