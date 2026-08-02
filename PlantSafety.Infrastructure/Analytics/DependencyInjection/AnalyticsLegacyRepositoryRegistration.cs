// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.DependencyInjection
//  Layer:          Infrastructure
//  File:           AnalyticsLegacyRepositoryRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the 22 pre-existing Analytics
//                  module repository interfaces (PlantSafety.Application.Analytics.*.Repositories)
//                  that were scaffolded with Dapper implementations under
//                  PlantSafety.Infrastructure\Analytics\ but never wired into the DI container.
//                  This gap was the reason ValidateOnBuild had to be disabled in
//                  PlantSafety.Api\Program.cs. Every repository class listed below already
//                  fully implements its interface (confirmed via a clean, zero-error build of
//                  PlantSafety.Infrastructure.csproj) and calls already-deployed stored
//                  procedures under the [Analytics] SQL schema — no new C# repository logic or
//                  T-SQL was required for this pass, only DI wiring.
//
//                  Deliberately separate from PlantSafety.Infrastructure\Features\Analytics\Dashboard,
//                  which is a different, already-working module tree and is NOT touched here.
//                  Call <c>services.AddAnalyticsLegacyRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.Behavior.Repositories;
using PlantSafety.Application.Analytics.Culture.CultureIndex.Repositories;
using PlantSafety.Application.Analytics.Culture.EngagementScoring.Repositories;
using PlantSafety.Application.Analytics.Culture.TrendModeling.Repositories;
using PlantSafety.Application.Analytics.Dashboard.Executive.Repositories;
using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Repositories;
using PlantSafety.Application.Analytics.Dashboard.ReadModel.Repositories;
using PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Repositories;
using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Repositories;
using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Repositories;
using PlantSafety.Application.Analytics.Predictive.RiskForecasting.Repositories;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Repositories;
using PlantSafety.Application.Analytics.Reporting.Repositories;
using PlantSafety.Application.Analytics.Repositories;
using PlantSafety.Application.Analytics.Schedules.Repositories;
using PlantSafety.Application.Analytics.ScoreCards.Compliance.Repositories;
using PlantSafety.Application.Analytics.ScoreCards.Department.Repositories;
using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Repositories;
using PlantSafety.Application.Analytics.ScoreCards.Training.Repositories;
using PlantSafety.Infrastructure.Analytics.Culture.Behavior.Persistence;
using PlantSafety.Infrastructure.Analytics.Culture.CultureIndex.Persistence;
using PlantSafety.Infrastructure.Analytics.Culture.EngagementScoring.Persistence;
using PlantSafety.Infrastructure.Analytics.Culture.TrendModeling.Persistence;
using PlantSafety.Infrastructure.Analytics.Dashboard.Executive.Persistence;
using PlantSafety.Infrastructure.Analytics.Dashboard.KPISnapshots.Persistence;
using PlantSafety.Infrastructure.Analytics.Dashboard.ReadModel.Persistence;
using PlantSafety.Infrastructure.Analytics.Dashboard.TrendAnalysis.Persistence;
using PlantSafety.Infrastructure.Analytics.Persistence;
using PlantSafety.Infrastructure.Analytics.Predictive.ComplianceFailure.Persistence;
using PlantSafety.Infrastructure.Analytics.Predictive.FatigueRiskModeling.Persistence;
using PlantSafety.Infrastructure.Analytics.Predictive.IncidentPrediction.Persistence;
using PlantSafety.Infrastructure.Analytics.Predictive.RiskForecasting.Persistence;
using PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Persistence;
using PlantSafety.Infrastructure.Analytics.Reporting.Persistence;
using PlantSafety.Infrastructure.Analytics.Schedules.Persistence;
using PlantSafety.Infrastructure.Analytics.ScoreCards.Compliance.Persistence;
using PlantSafety.Infrastructure.Analytics.ScoreCards.Department.Persistence;
using PlantSafety.Infrastructure.Analytics.ScoreCards.Supervisor.Persistence;
using PlantSafety.Infrastructure.Analytics.ScoreCards.Training.Persistence;
using PlantSafety.Infrastructure.Analytics.TrendAnalysis.Persistence;
using PlantSafety.Application.Analytics.Culture.Repositories;
using PlantSafety.Infrastructure.Analytics.Culture.Persistence;
using PlantSafety.Application.Analytics.Dashboard.Repositories;
using PlantSafety.Domain.Analytics.Dashboard.Interfaces;
using PlantSafety.Infrastructure.Analytics.Dashboard.Persistence;

namespace PlantSafety.Infrastructure.Analytics.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the 22 pre-existing Analytics module repository interfaces
/// that were scaffolded with Dapper implementations but never registered in the DI container.
/// </summary>
public static class AnalyticsLegacyRepositoryRegistration
{
    /// <summary>Registers all 22 legacy Analytics Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddAnalyticsLegacyRepositories(this IServiceCollection services)
    {
        // Culture subsystem
        services.AddScoped<Application.Common.Interfaces.Persistence.ICultureRepository, CultureRepository>();
        // Phase 4 full build-out (2026-08-02): Application-layer read-model repository for the new
        // GetCultureByIdQuery, distinct from the write-side ICultureRepository registered above.
        services.AddScoped<ICultureReadRepository, CultureReadRepository>();
        services.AddScoped<IBehaviorRepository, BehaviorDapperRepository>();
        services.AddScoped<ICultureIndexRepository, CultureIndexDapperRepository>();
        services.AddScoped<IEngagementScoringRepository, EngagementScoringDapperRepository>();
        services.AddScoped<ITrendModelingRepository, TrendModelingDapperRepository>();

        // Dashboard subsystem
        // Phase 4 full build-out (2026-08-02): write-side Domain aggregate repository and
        // Application-layer read-model repository for the Dashboard root aggregate. This aggregate
        // previously had zero persistence wiring at all -- no table, no stored procedures, and no
        // DI registration.
        services.AddScoped<IDashboardRepository, DashboardAggregateDapperRepository>();
        services.AddScoped<IDashboardReadRepository, DashboardReadRepository>();
        services.AddScoped<IExecutiveRepository, ExecutiveDapperRepository>();
        services.AddScoped<IKPISnapshotsRepository, KPISnapshotsDapperRepository>();
        services.AddScoped<IReadModelRepository, ReadModelDapperRepository>();
        services.AddScoped<
            Application.Analytics.Dashboard.TrendAnalysis.Repositories.ITrendAnalysisRepository,
            DashboardTrendAnalysisDapperRepository>();

        // Predictive subsystem
        services.AddScoped<IComplianceFailureRepository, ComplianceFailureDapperRepository>();
        services.AddScoped<IFatigueRiskModelingRepository, FatigueRiskModelingDapperRepository>();
        services.AddScoped<IIncidentPredictionRepository, IncidentPredictionDapperRepository>();
        services.AddScoped<IRiskForecastingRepository, RiskForecastingDapperRepository>();
        services.AddScoped<IPredictiveAnalyticsRepository, PredictiveAnalyticsDapperRepository>();

        // Reporting / Schedules / root-level Analytics repositories
        services.AddScoped<IReportingRepository, ReportingDapperRepository>();
        services.AddScoped<IEngagementRepository, EngagementDapperRepository>();
        services.AddScoped<IExportAnalyticsRepository, ExportAnalyticsDapperRepository>();
        services.AddScoped<
            ISchedulesRepository,
            PlantSafety.Infrastructure.Analytics.Schedules.Persistence.SchedulesDapperRepository>();

        // ScoreCards subsystem
        services.AddScoped<IComplianceRepository, ComplianceDapperRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentDapperRepository>();
        services.AddScoped<ISupervisorRepository, SupervisorDapperRepository>();
        services.AddScoped<ITrainingRepository, TrainingDapperRepository>();

        // Root-level TrendAnalysis subsystem (distinct from Dashboard.TrendAnalysis above)
        services.AddScoped<
            Application.Analytics.TrendAnalysis.Repositories.ITrendAnalysisRepository,
            TrendAnalysisDapperRepository>();

        return services;
    }
}
