// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork
//  Layer:          Infrastructure
//  File:           PermitToWorkInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the PermitToWork Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.PermitToWork.PermitExpiration.Persistence;
using PlantSafety.Infrastructure.PermitToWork.PermitScheduling.Persistence;
using PlantSafety.Infrastructure.PermitToWork.PermitTemplate.Persistence;
using PlantSafety.Infrastructure.PermitToWork.PermitWorkflow.Persistence;
using PlantSafety.Application.PermitToWork.Commands;
using PlantSafety.Infrastructure.PermitToWork.Commands.Persistence;

namespace PlantSafety.Infrastructure.PermitToWork.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the PermitToWork module.</summary>
public static class PermitToWorkInfrastructureServiceRegistration
{
    /// <summary>Adds PermitToWork Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPermitToWorkInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.PermitToWork.PermitExpiration.Interfaces.IPermitExpirationRepository, PermitExpirationDapperRepository>();
        services.AddScoped<PlantSafety.Application.PermitToWork.PermitExpiration.Repositories.IPermitExpirationRepository, PermitExpirationReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PermitToWork.PermitScheduling.Interfaces.IPermitSchedulingRepository, PermitSchedulingDapperRepository>();
        services.AddScoped<PlantSafety.Application.PermitToWork.PermitScheduling.Repositories.IPermitSchedulingRepository, PermitSchedulingReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PermitToWork.PermitTemplate.Interfaces.IPermitTemplateRepository, PermitTemplateDapperRepository>();
        services.AddScoped<PlantSafety.Application.PermitToWork.PermitTemplate.Repositories.IPermitTemplateRepository, PermitTemplateReadDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces.IPermitWorkflowRepository, PermitWorkflowDapperRepository>();
        services.AddScoped<PlantSafety.Application.PermitToWork.PermitWorkflow.Repositories.IPermitWorkflowRepository, PermitWorkflowReadDapperRepository>();
        services.AddScoped<IPermitToWorkBulkRepository, PermitToWorkBulkDapperRepository>();

        return services;
    }
}
