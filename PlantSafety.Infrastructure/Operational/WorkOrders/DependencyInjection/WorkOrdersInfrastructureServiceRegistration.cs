// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Infrastructure
//  File:           WorkOrdersInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Operational.WorkOrders
//                  Infrastructure-layer repository implementations (write-side aggregate
//                  repositories for WorkOrder/Part, plus the Application-layer read repositories).
//                  Mirrors PreventiveMaintenanceInfrastructureServiceRegistration.cs. Wired into the
//                  central InfrastructureServiceRegistration.cs directly. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Operational.WorkOrders.Persistence;

namespace PlantSafety.Infrastructure.Operational.WorkOrders.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Operational.WorkOrders module.</summary>
public static class WorkOrdersInfrastructureServiceRegistration
{
    /// <summary>Adds WorkOrder/Part Dapper repositories (write-side and read-side) to the service collection.</summary>
    public static IServiceCollection AddWorkOrdersInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Operational.WorkOrders.Interfaces.IWorkOrderRepository, WorkOrderDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Operational.WorkOrders.Interfaces.IPartRepository, PartDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operational.WorkOrders.Repositories.IWorkOrderReadRepository, WorkOrderReadDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operational.WorkOrders.Repositories.IPartInventoryReadRepository, PartInventoryReadDapperRepository>();

        // Phase 6 §3.4 Replay/Rebuild Across Systems (2026-08-03, Eventing & Messaging Agent):
        // point-in-time WorkOrder state reconstruction directly from [System].[OutboxMessages].
        services.AddScoped<PlantSafety.Application.Operational.WorkOrders.Repositories.IWorkOrderEventReplayRepository, WorkOrderEventReplayDapperRepository>();

        return services;
    }
}
