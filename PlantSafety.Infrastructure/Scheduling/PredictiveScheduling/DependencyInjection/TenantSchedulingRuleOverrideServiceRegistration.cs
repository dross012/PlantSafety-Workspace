// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Infrastructure
//  File:           TenantSchedulingRuleOverrideServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §1.4 Tenant-Aware Scheduling)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the tenant scheduling rule override
//                  provider. NOT wired into the shared InfrastructureServiceRegistration.cs -- a
//                  human wires this extension in centrally after all module-scoped agents finish
//                  (matching the AuditReport submodule's established precedent). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;

namespace PlantSafety.Infrastructure.Scheduling.PredictiveScheduling.DependencyInjection;

/// <summary>Registers the Phase 7 §1.4 tenant scheduling rule override provider.</summary>
public static class TenantSchedulingRuleOverrideServiceRegistration
{
    /// <summary>Binds <see cref="TenantSchedulingRuleOverrideOptions"/> from configuration and registers <see cref="ITenantSchedulingRuleOverrideProvider"/>.</summary>
    public static IServiceCollection AddTenantSchedulingRuleOverrides(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TenantSchedulingRuleOverrideOptions>(configuration.GetSection(TenantSchedulingRuleOverrideOptions.SectionName));
        services.AddSingleton<ITenantSchedulingRuleOverrideProvider, TenantSchedulingRuleOverrideProvider>();

        return services;
    }
}
