// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing
//  Layer:          Infrastructure
//  File:           BillingInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3 Billing, Metering & Subscription Management)
//  SchemaVersion:  2026.1
//  Description:    Registers Infrastructure-layer services for the Billing module: usage-metering
//                  snapshot persistence, invoice persistence, the tenant storage-usage provider,
//                  and the payment-gateway extension seam (UnconfiguredPaymentProcessor by
//                  default -- see that class's remarks for how to plug in a real gateway).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Payments;
using PlantSafety.Application.Abstractions.RateLimiting;
using PlantSafety.Application.Billing.Metering.Abstractions;
using PlantSafety.Domain.Billing.BillingEngine.Interfaces;
using PlantSafety.Domain.Billing.Metering.Interfaces;
using PlantSafety.Infrastructure.Billing.BillingEngine.Persistence;
using PlantSafety.Infrastructure.Billing.Metering;
using PlantSafety.Infrastructure.Billing.Metering.Persistence;
using PlantSafety.Infrastructure.Billing.PaymentProcessing;
using PlantSafety.Infrastructure.Billing.RateLimiting;

namespace PlantSafety.Infrastructure.Billing.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Billing module.</summary>
public static class BillingInfrastructureServiceRegistration
{
    /// <summary>Adds Billing Dapper repositories, the tenant storage-usage provider, and the payment-processor extension seam.</summary>
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUsageMeteringSnapshotRepository, UsageMeteringSnapshotDapperRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceDapperRepository>();
        services.AddScoped<ITenantStorageUsageProvider, BlobTenantStorageUsageProvider>();

        // Payment gateway (Phase 7 §3.3) -- deliberate "bring your own payment gateway" extension
        // seam. No payment gateway SDK is referenced anywhere in this solution; see
        // UnconfiguredPaymentProcessor's remarks for how to wire in a real implementation without
        // touching any other layer.
        services.AddScoped<IPaymentProcessor, UnconfiguredPaymentProcessor>();

        // Phase 7 §3.4 License Enforcement -- per-tenant API rate limiting. Singleton cache
        // (must survive across the per-request scoped lifetime and be readable synchronously by
        // the rate limiter's partition-key factory) plus the background service that keeps it
        // fresh from each tenant's active SubscriptionPlan. See ITenantRateLimitCache remarks.
        services.AddSingleton<ITenantRateLimitCache, InMemoryTenantRateLimitCache>();
        services.AddHostedService<TenantRateLimitCacheRefreshService>();

        return services;
    }
}
