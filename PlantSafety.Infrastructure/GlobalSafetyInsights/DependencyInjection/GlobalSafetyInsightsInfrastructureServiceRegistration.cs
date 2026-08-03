// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyInsightsInfrastructureServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2/§4 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the GlobalSafetyInsights Infrastructure-
//                  layer repository/publisher implementations. MUST be invoked from
//                  InfrastructureServiceRegistration.AddInfrastructure -- see that file's call
//                  site (this exact "registered but never wired" bug has recurred repeatedly
//                  across this session's other agents; verified wired here). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Publishers;
using PlantSafety.Application.GlobalSafetyInsights.Publishers;
using PlantSafety.Application.GlobalSafetyInsights.Repositories;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Publishers;
using PlantSafety.Infrastructure.GlobalSafetyInsights.Persistence;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the GlobalSafetyInsights module.</summary>
public static class GlobalSafetyInsightsInfrastructureServiceRegistration
{
    /// <summary>Adds GlobalSafetyInsights Dapper repositories and the mesh publishers to the service collection.</summary>
    public static IServiceCollection AddGlobalSafetyInsightsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IGlobalSafetyInsightsRepository, GlobalSafetyInsightsDapperRepository>();
        services.AddScoped<IGlobalSafetyInsightsReadRepository, GlobalSafetyInsightsReadDapperRepository>();
        services.AddScoped<IGlobalSafetyActionAuditRepository, GlobalSafetyActionAuditDapperRepository>();
        services.AddScoped<IGlobalSafetyInsightsPublisher, GlobalSafetyInsightsPublisher>();

        // Phase 8 §3 Multi-Site Coordination Workflows: publishes CrossSiteActionEvent onto the
        // real Outbox -> MassTransit pipeline for CrossSiteActionConsumer to record into the audit ledger.
        services.AddScoped<ICrossSiteActionEventPublisher, CrossSiteActionEventPublisher>();

        return services;
    }
}
