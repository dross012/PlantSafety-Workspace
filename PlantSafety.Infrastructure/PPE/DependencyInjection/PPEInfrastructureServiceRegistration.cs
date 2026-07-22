// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE
//  Layer:          Infrastructure
//  File:           PPEInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the PPE Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.PPE.PPEAssignment.Persistence;
using PlantSafety.Infrastructure.PPE.PPECategory.Persistence;
using PlantSafety.Infrastructure.PPE.PPEInventory.Persistence;
using PlantSafety.Infrastructure.PPE.PPETrainingFit.Persistence;
using PlantSafety.Infrastructure.PPE.PPEZoneMapping.Persistence;
using PlantSafety.Application.PPE.Commands;
using PlantSafety.Infrastructure.PPE.Commands.Persistence;

namespace PlantSafety.Infrastructure.PPE.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the PPE module.</summary>
public static class PPEInfrastructureServiceRegistration
{
    /// <summary>Adds PPE Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPPEInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.PPE.PPEAssignment.Interfaces.IPPEAssignmentRepository, PPEAssignmentDapperRepository>();
        services.AddScoped<PlantSafety.Application.PPE.PPEAssignment.Repositories.IPPEAssignmentRepository, PPEAssignmentReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PPE.PPECategory.Interfaces.IPPECategoryRepository, PPECategoryDapperRepository>();
        services.AddScoped<PlantSafety.Application.PPE.PPECategory.Repositories.IPPECategoryRepository, PPECategoryReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PPE.PPEInventory.Interfaces.IPPEInventoryRepository, PPEInventoryDapperRepository>();
        services.AddScoped<PlantSafety.Application.PPE.PPEInventory.Repositories.IPPEInventoryRepository, PPEInventoryReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PPE.PPETrainingFit.Interfaces.IPPETrainingFitRepository, PPETrainingFitDapperRepository>();
        services.AddScoped<PlantSafety.Application.PPE.PPETrainingFit.Repositories.IPPETrainingFitRepository, PPETrainingFitReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PPE.PPEZoneMapping.Interfaces.IPPEZoneMappingRepository, PPEZoneMappingDapperRepository>();
        services.AddScoped<PlantSafety.Application.PPE.PPEZoneMapping.Repositories.IPPEZoneMappingRepository, PPEZoneMappingReadDapperRepository>();
        services.AddScoped<IPPEBulkRepository, PPEBulkDapperRepository>();

        return services;
    }
}
