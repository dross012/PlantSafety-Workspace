// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling
//  Layer:          Infrastructure
//  File:           SchedulingInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Scheduling Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Scheduling.PMScheduling.Persistence;
using PlantSafety.Infrastructure.Scheduling.EmergencyDrillPlanning.Persistence;
using PlantSafety.Infrastructure.Scheduling.HazardIsolationSequencing.Persistence;
using PlantSafety.Infrastructure.Scheduling.ShiftFatigue.Persistence;
using PlantSafety.Infrastructure.Scheduling.WorkOrderPriority.Persistence;
using PlantSafety.Infrastructure.Scheduling.PredictiveScheduling.Persistence;
using PlantSafety.Application.Scheduling.Commands;
using PlantSafety.Infrastructure.Scheduling.Commands.Persistence;

namespace PlantSafety.Infrastructure.Scheduling.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Scheduling module.</summary>
public static class SchedulingInfrastructureServiceRegistration
{
    /// <summary>Adds Scheduling Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddSchedulingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Scheduling.PMScheduling.Interfaces.IPMSchedulingRepository, PMSchedulingDapperRepository>();
        services.AddScoped<PlantSafety.Application.Scheduling.PMScheduling.Repositories.IPMSchedulingRepository, PMSchedulingReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces.IEmergencyDrillPlanningRepository, EmergencyDrillPlanningDapperRepository>();
        services.AddScoped<PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories.IEmergencyDrillPlanningRepository, EmergencyDrillPlanningReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces.IHazardIsolationSequencingRepository, HazardIsolationSequencingDapperRepository>();
        services.AddScoped<PlantSafety.Application.Scheduling.HazardIsolationSequencing.Repositories.IHazardIsolationSequencingRepository, HazardIsolationSequencingReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Scheduling.ShiftFatigue.Interfaces.IShiftFatigueRepository, ShiftFatigueDapperRepository>();
        services.AddScoped<PlantSafety.Application.Scheduling.ShiftFatigue.Repositories.IShiftFatigueRepository, ShiftFatigueReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces.IWorkOrderPriorityRepository, WorkOrderPriorityDapperRepository>();
        services.AddScoped<PlantSafety.Application.Scheduling.WorkOrderPriority.Repositories.IWorkOrderPriorityRepository, WorkOrderPriorityReadDapperRepository>();
        services.AddScoped<ISchedulingBulkRepository, SchedulingBulkDapperRepository>();

        // Phase 6 full build-out: OptimizationRun aggregate persistence (was previously a pure
        // stateless computation engine with no persistence layer -- see PredictiveScheduling
        // module header comments).
        services.AddScoped<PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces.IOptimizationRunRepository, OptimizationRunDapperRepository>();
        services.AddScoped<PlantSafety.Application.Scheduling.PredictiveScheduling.Repositories.IOptimizationRunReadRepository, OptimizationRunReadDapperRepository>();

        return services;
    }
}
