// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Infrastructure
//  File:           ProductionTrackingInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Operational.ProductionTracking
//                  Infrastructure-layer repository implementations (write-side aggregate
//                  repositories for ProductionLine/ProductionRun, plus the Application-layer read
//                  repository). Mirrors WorkOrdersInfrastructureServiceRegistration.cs. Wired into
//                  the central InfrastructureServiceRegistration.cs directly. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Infrastructure.Operational.ProductionTracking.Persistence;

namespace PlantSafety.Infrastructure.Operational.ProductionTracking.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Operational.ProductionTracking module.</summary>
public static class ProductionTrackingInfrastructureServiceRegistration
{
    /// <summary>Adds ProductionLine/ProductionRun Dapper repositories (write-side and read-side) to the service collection.</summary>
    public static IServiceCollection AddProductionTrackingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Operational.ProductionTracking.Interfaces.IProductionLineRepository, ProductionLineDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Operational.ProductionTracking.Interfaces.IProductionRunRepository, ProductionRunDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operational.ProductionTracking.Repositories.IProductionTrackingReadRepository, ProductionTrackingReadDapperRepository>();

        return services;
    }
}
