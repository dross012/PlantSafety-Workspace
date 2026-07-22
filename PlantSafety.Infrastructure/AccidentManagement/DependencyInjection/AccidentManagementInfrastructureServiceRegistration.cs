// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Infrastructure
//  File:           AccidentManagementInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the AccidentManagement Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Repositories;
using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Repositories;
using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Repositories;
using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Repositories;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Infrastructure.AccidentManagement.Persistence;

namespace PlantSafety.Infrastructure.AccidentManagement.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the AccidentManagement module.</summary>
public static class AccidentManagementInfrastructureServiceRegistration
{
    /// <summary>Adds AccidentManagement Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddAccidentManagementInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAccidentRepository, AccidentDapperRepository>();

        services.AddScoped<AccidentManagementCategoryDapperRepository>();
        services.AddScoped<ISiteSecuringAndTriageRepository>(sp => sp.GetRequiredService<AccidentManagementCategoryDapperRepository>());
        services.AddScoped<IInvestigationRootCauseAnalysisRepository>(sp => sp.GetRequiredService<AccidentManagementCategoryDapperRepository>());
        services.AddScoped<ICorrectiveActionsCAPARepository>(sp => sp.GetRequiredService<AccidentManagementCategoryDapperRepository>());
        services.AddScoped<IRegulatoryComplianceDocumentationRepository>(sp => sp.GetRequiredService<AccidentManagementCategoryDapperRepository>());

        return services;
    }
}
