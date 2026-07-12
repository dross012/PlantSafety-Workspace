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
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Security;
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
using PlantSafety.Application.Features.SafetyEvents.Handlers.RootCauseAnalysis;
using PlantSafety.Infrastructure.Features.SafetyEvents.RootCauseAnalysis.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.Exports;
using PlantSafety.Infrastructure.Features.SafetyEvents.Exports.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;
using PlantSafety.Infrastructure.Features.SafetyEvents.DrillsAndExercises.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;
using PlantSafety.Infrastructure.Features.SafetyEvents.EmergencyManagement.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;
using PlantSafety.Infrastructure.Features.SafetyEvents.CrisisManagement.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;
using PlantSafety.Infrastructure.Features.SafetyEvents.Observation.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;
using PlantSafety.Infrastructure.Features.SafetyEvents.NearMisses.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories;
using PlantSafety.Application.Features.Compliance.Handlers.Permits;
using PlantSafety.Infrastructure.Features.Compliance.Permits.Repositories;
using PlantSafety.Application.Features.Compliance.Handlers.ManagementOfChange;
using PlantSafety.Infrastructure.Features.Compliance.ManagementOfChange.Repositories;
using PlantSafety.Application.Features.Analytics.Handlers.Dashboard;
using PlantSafety.Infrastructure.Features.Analytics.Dashboard.Repositories;
using PlantSafety.Application.Features.PlanningProjects.Handlers.Projects;
using PlantSafety.Infrastructure.Features.PlanningProjects.Projects.Repositories;
using PlantSafety.Application.Features.Financials.Handlers.Budgets;
using PlantSafety.Infrastructure.Features.Financials.Budgets.Repositories;
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;
using PlantSafety.Infrastructure.Features.EnterpriseFabric.Users.Repositories;
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;
using PlantSafety.Infrastructure.Features.ContinuousImprovement.CAPA.Repositories;
using PlantSafety.Application.Features.Operational.Handlers.Training;
using PlantSafety.Infrastructure.Features.Operational.Training.Repositories;

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
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionService, PermissionService>();

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

        // Features tree — SafetyEvents module repositories
        services.AddScoped<IRcaRepository, RcaDapperRepository>();
        services.AddScoped<IExportJobRepository, ExportJobDapperRepository>();
        services.AddScoped<IDrillRepository, DrillDapperRepository>();
        services.AddScoped<IEmergencyResponseRepository, EmergencyResponseDapperRepository>();
        services.AddScoped<ICrisisEventRepository, CrisisEventDapperRepository>();
        services.AddScoped<IObservationRepository, ObservationDapperRepository>();
        services.AddScoped<INearMissRepository, NearMissDapperRepository>();
        services.AddScoped<IIncidentReadRepository, IncidentReadDapperRepository>();
        services.AddScoped<IIncidentRepository, IncidentDapperRepository>();

        // Features tree — Compliance module repositories
        services.AddScoped<IPermitRepository, PermitDapperRepository>();
        services.AddScoped<IMocRepository, MocDapperRepository>();

        // Features tree — Analytics module repositories
        services.AddScoped<IDashboardRepository, DashboardDapperRepository>();

        // Features tree — PlanningProjects module repositories
        services.AddScoped<IProjectRepository, ProjectDapperRepository>();

        // Features tree — Financials module repositories
        services.AddScoped<IBudgetRepository, BudgetDapperRepository>();

        // Features tree — EnterpriseFabric module repositories
        services.AddScoped<IUserRepository, UserDapperRepository>();

        // Features tree — ContinuousImprovement module repositories
        services.AddScoped<ICorrectiveActionRepository, CorrectiveActionDapperRepository>();

        // Features tree — Operational module repositories
        services.AddScoped<ITrainingRepository, TrainingDapperRepository>();

        return services;
    }
}
