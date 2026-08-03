// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Infrastructure
//  File:           ComplianceRulesEngineInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance Rules Engine
//                  Infrastructure-layer repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish, mirroring
//                  RiskManagementInfrastructureServiceRegistration's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.RulesEngine.Interfaces;
using PlantSafety.Application.Compliance.RulesEngine.Repositories;
using PlantSafety.Infrastructure.Compliance.RulesEngine.Persistence;

namespace PlantSafety.Infrastructure.Compliance.RulesEngine.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance Rules Engine submodule.</summary>
public static class ComplianceRulesEngineInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance Rules Engine Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddComplianceRulesEngineInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IComplianceEvaluationRepository, ComplianceEvaluationDapperRepository>();
        services.AddScoped<IComplianceEvaluationReadRepository, ComplianceEvaluationReadDapperRepository>();

        return services;
    }
}
