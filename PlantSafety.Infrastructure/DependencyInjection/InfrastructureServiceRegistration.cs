// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           InfrastructureServiceRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Canonical DI registration entry point for all Infrastructure services.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using PlantSafety.Infrastructure.BackgroundJobs;
using PlantSafety.Infrastructure.Caching;
using PlantSafety.Infrastructure.Common.Extensions;
using PlantSafety.Infrastructure.Common.Mapping;
using PlantSafety.Infrastructure.ExternalServices;
using PlantSafety.Infrastructure.FileStorage;
using PlantSafety.Infrastructure.Financials.Persistence;
using PlantSafety.Infrastructure.Financials.Services;
using PlantSafety.Infrastructure.Identity;
using PlantSafety.Infrastructure.Messaging.Publishers;
using PlantSafety.Infrastructure.OperationalSafety.Alerts.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.Alerts.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.Checklists.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.Checklists.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.Schedules.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.Schedules.Repositories;
using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
using PlantSafety.Infrastructure.Persistence.Dapper.Commands;
using PlantSafety.Infrastructure.Persistence.Dapper.Queries;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;
using PlantSafety.Infrastructure.PlanningProjects.Persistence;
using PlantSafety.Infrastructure.PlanningProjects.Services;
using PlantSafety.Infrastructure.Search;
using PlantSafety.Infrastructure.Training.CompetencyManagement.Interfaces;
using PlantSafety.Infrastructure.Training.CompetencyManagement.Repositories;

namespace PlantSafety.Infrastructure.DependencyInjection;

/// <summary>
/// Canonical DI entry point for PlantSafety.Infrastructure.
/// Call <c>services.AddInfrastructure(configuration)</c> from the host startup.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>Registers all Infrastructure services into the DI container.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options validation
        services.AddInfrastructureOptionsValidation();

        // Dapper type handlers (idempotent; safe to call multiple times)
        DapperTypeHandlers.RegisterAll();

        // Persistence
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<DapperCommandExecutor>();
        services.AddScoped<DapperQueryExecutor>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Caching
        services.AddStackExchangeRedisCache(opts =>
            opts.Configuration = configuration.GetConnectionString("Redis"));
        services.AddScoped<ICacheService, DistributedCacheService>();

        // Identity
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenValidationService, TokenValidationService>();
        services.AddScoped<ActorContextResolver>();

        // File storage
        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        // Search
        services.AddScoped<ISearchService, AzureSearchService>();

        // Messaging
        services.AddScoped<IIntegrationEventPublisher, ServiceBusEventPublisher>();

        // Background jobs
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<ProcessedEventsCleaner>();

        // OperationalSafety module repositories
        services.AddScoped<IAlertsInfrastructure, AlertsDapperRepository>();
        services.AddScoped<IChecklistsInfrastructure, ChecklistsDapperRepository>();
        services.AddScoped<IPPEManagementInfrastructure, PPEManagementDapperRepository>();
        services.AddScoped<IRiskManagementInfrastructure, RiskManagementDapperRepository>();
        services.AddScoped<ISchedulesInfrastructure, SchedulesDapperRepository>();

        // Training module repositories
        services.AddScoped<ICompetencyManagementInfrastructure, CompetencyManagementDapperRepository>();

        // Financials
        services.AddScoped<FinancialsRepository>();
        services.AddScoped<FinancialsService>();

        // PlanningProjects
        services.AddScoped<PlanningProjectsRepository>();
        services.AddScoped<PlanningProjectsService>();

        return services;
    }
}
