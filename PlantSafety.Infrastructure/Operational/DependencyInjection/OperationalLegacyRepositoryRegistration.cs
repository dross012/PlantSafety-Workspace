// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.DependencyInjection
//  Layer:          Infrastructure
//  File:           OperationalLegacyRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the pre-existing Operational
//                  module repository interfaces (PlantSafety.Application.Operational.*.Repositories)
//                  that were scaffolded in the Application layer but never given Infrastructure
//                  implementations. This gap was one of the reasons ValidateOnBuild had to be
//                  disabled in PlantSafety.Api\Program.cs.
//
//                  This file covers all 26 pre-existing Operational module repository interfaces:
//                  Alerts, Checklists, CompetencyManagement, Contractor, EquipmentSafety,
//                  Ergonomics, FatigueManagement, HazardAssessments, HazardousMaterials,
//                  IndustrialHygiene, Inspections, JobSafetyAnalysis, LockoutTagout, PPEManagement,
//                  PermitsToWork, PredictiveMaintenance, PreventiveMaintenance, RiskManagement,
//                  Schedules, Training (+ Training.Classes, Training.Courses, Training.Seminars,
//                  Training.Webinars), the cross-cutting IOperationalBulkRepository, and the
//                  cross-cutting IExportOperationalRepository.
//
//                  Of these 26, only 7 (Alerts, Checklists, Contractor, PPEManagement,
//                  RiskManagement, Schedules, Training) had ANY pre-existing SQL objects deployed
//                  under [Operational] ("sp_*" prefix, from an earlier/unrelated scaffolding pass).
//                  Those legacy "sp_*" objects use a fundamentally different contract (TenantId +
//                  paging only, no SiteId/date-range filtering, and column shapes that do not match
//                  the Application-layer DTOs) and were NOT reused. Instead, for consistency with the
//                  established sibling pattern (see PlantSafety.Infrastructure\Compliance\Sql\), all
//                  26 interfaces were backed by brand-new "*ReadModel" tables + "usp_*_List" (or
//                  usp_OperationalResources_BulkUpdate / usp_OperationalExportItems_List) stored
//                  procedures under PlantSafety.Infrastructure\Operational\Sql\{Tables,Procedures}\,
//                  deployed to the live localhost\PLANTSAFETY -> PlantSafety.CoreDB database as part
//                  of this pass. All 26 required brand-new SQL (no genuine pre-existing match).
//
//                  Call <c>services.AddOperationalLegacyRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents working the same file). Additive-only
//                  evolution.
//
//                  2026-08-20 addendum: three more genuinely-new Operational list-view modules
//                  (EnvironmentalMonitoring, Wellness, WorkforceSafety) were scaffolded end-to-end
//                  following this exact EquipmentSafety/Webinars convention and are registered
//                  below alongside the original 26. Bringing the total to 29.
// ============================================================================
using PlantSafety.Application.Operational.Alerts.Repositories;
using PlantSafety.Application.Operational.Checklists.Repositories;
using PlantSafety.Application.Operational.Commands;
using PlantSafety.Application.Operational.CompetencyManagement.Repositories;
using PlantSafety.Application.Operational.Contractor.Repositories;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Repositories;
using PlantSafety.Application.Operational.EquipmentSafety.Repositories;
using PlantSafety.Application.Operational.Ergonomics.Repositories;
using PlantSafety.Application.Operational.FatigueManagement.Repositories;
using PlantSafety.Application.Operational.HazardAssessments.Repositories;
using PlantSafety.Application.Operational.HazardousMaterials.Repositories;
using PlantSafety.Application.Operational.IndustrialHygiene.Repositories;
using PlantSafety.Application.Operational.Inspections.Repositories;
using PlantSafety.Application.Operational.JobSafetyAnalysis.Repositories;
using PlantSafety.Application.Operational.LockoutTagout.Repositories;
using PlantSafety.Application.Operational.PermitsToWork.Repositories;
using PlantSafety.Application.Operational.PPEManagement.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;
using PlantSafety.Application.Operational.Repositories;
using PlantSafety.Application.Operational.RiskManagement.Repositories;
using PlantSafety.Application.Operational.Schedules.Repositories;
using PlantSafety.Application.Operational.Training.Classes.Repositories;
using PlantSafety.Application.Operational.Training.Courses.Repositories;
using PlantSafety.Application.Operational.Training.Repositories;
using PlantSafety.Application.Operational.Training.Seminars.Repositories;
using PlantSafety.Application.Operational.Training.Webinars.Repositories;
using PlantSafety.Application.Operational.Wellness.Repositories;
using PlantSafety.Application.Operational.WorkforceSafety.Repositories;
using PlantSafety.Infrastructure.Operational.Alerts.Persistence;
using PlantSafety.Infrastructure.Operational.Checklists.Persistence;
using PlantSafety.Infrastructure.Operational.Commands.Persistence;
using PlantSafety.Infrastructure.Operational.CompetencyManagement.Persistence;
using PlantSafety.Infrastructure.Operational.Contractor.Persistence;
using PlantSafety.Infrastructure.Operational.EnvironmentalMonitoring.Persistence;
using PlantSafety.Infrastructure.Operational.EquipmentSafety.Persistence;
using PlantSafety.Infrastructure.Operational.Ergonomics.Persistence;
using PlantSafety.Infrastructure.Operational.FatigueManagement.Persistence;
using PlantSafety.Infrastructure.Operational.HazardAssessments.Persistence;
using PlantSafety.Infrastructure.Operational.HazardousMaterials.Persistence;
using PlantSafety.Infrastructure.Operational.IndustrialHygiene.Persistence;
using PlantSafety.Infrastructure.Operational.Inspections.Persistence;
using PlantSafety.Infrastructure.Operational.JobSafetyAnalysis.Persistence;
using PlantSafety.Infrastructure.Operational.LockoutTagout.Persistence;
using PlantSafety.Infrastructure.Operational.PermitsToWork.Persistence;
using PlantSafety.Infrastructure.Operational.PPEManagement.Persistence;
using PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence;
using PlantSafety.Infrastructure.Operational.PreventiveMaintenance.Persistence;
using PlantSafety.Infrastructure.Operational.Repositories.Persistence;
using PlantSafety.Infrastructure.Operational.RiskManagement.Persistence;
using PlantSafety.Infrastructure.Operational.Schedules.Persistence;
using PlantSafety.Infrastructure.Operational.Training.Classes.Persistence;
using PlantSafety.Infrastructure.Operational.Training.Courses.Persistence;
using PlantSafety.Infrastructure.Operational.Training.Persistence;
using PlantSafety.Infrastructure.Operational.Training.Seminars.Persistence;
using PlantSafety.Infrastructure.Operational.Training.Webinars.Persistence;
using PlantSafety.Infrastructure.Operational.Wellness.Persistence;
using PlantSafety.Infrastructure.Operational.WorkforceSafety.Persistence;

namespace PlantSafety.Infrastructure.Operational.DependencyInjection;

/// <summary>
/// Per-module DI entry point for all 26 pre-existing Operational module repository interfaces
/// that were scaffolded with Application-layer contracts but never given Infrastructure
/// implementations.
/// </summary>
public static class OperationalLegacyRepositoryRegistration
{
    /// <summary>Registers all 26 legacy Operational Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddOperationalLegacyRepositories(this IServiceCollection services)
    {
        // Core Operational read-model repositories (backed by pre-existing "sp_*" tables/data,
        // wired here against brand-new "*ReadModel" tables/usp_*_List procs — see file header).
        services.AddScoped<IAlertsRepository, PlantSafety.Infrastructure.Operational.Alerts.Persistence.AlertsDapperRepository>();
        services.AddScoped<IChecklistsRepository, PlantSafety.Infrastructure.Operational.Checklists.Persistence.ChecklistsDapperRepository>();
        services.AddScoped<IContractorRepository, ContractorDapperRepository>();
        services.AddScoped<IPPEManagementRepository, PlantSafety.Infrastructure.Operational.PPEManagement.Persistence.PPEManagementDapperRepository>();
        services.AddScoped<IRiskManagementRepository, PlantSafety.Infrastructure.Operational.RiskManagement.Persistence.RiskManagementDapperRepository>();
        services.AddScoped<ISchedulesRepository, PlantSafety.Infrastructure.Operational.Schedules.Persistence.SchedulesDapperRepository>();
        services.AddScoped<
            Application.Operational.Training.Repositories.ITrainingRepository,
            PlantSafety.Infrastructure.Operational.Training.Persistence.TrainingDapperRepository>();

        // Operational read-model repositories with no pre-existing SQL objects at all
        services.AddScoped<ICompetencyManagementRepository, PlantSafety.Infrastructure.Operational.CompetencyManagement.Persistence.CompetencyManagementDapperRepository>();
        services.AddScoped<IEquipmentSafetyRepository, EquipmentSafetyDapperRepository>();
        services.AddScoped<IErgonomicsRepository, ErgonomicsDapperRepository>();
        services.AddScoped<IFatigueManagementRepository, FatigueManagementDapperRepository>();
        services.AddScoped<IHazardAssessmentsRepository, HazardAssessmentsDapperRepository>();
        services.AddScoped<IHazardousMaterialsRepository, HazardousMaterialsDapperRepository>();
        services.AddScoped<IIndustrialHygieneRepository, IndustrialHygieneDapperRepository>();
        services.AddScoped<IInspectionsRepository, InspectionsDapperRepository>();
        services.AddScoped<IJobSafetyAnalysisRepository, JobSafetyAnalysisDapperRepository>();
        services.AddScoped<ILockoutTagoutRepository, LockoutTagoutDapperRepository>();
        services.AddScoped<IPermitsToWorkRepository, PermitsToWorkDapperRepository>();
        services.AddScoped<IPredictiveMaintenanceRepository, PredictiveMaintenanceDapperRepository>();
        services.AddScoped<IPreventiveMaintenanceRepository, PreventiveMaintenanceDapperRepository>();

        // Training sub-aggregates
        services.AddScoped<IClassesRepository, ClassesDapperRepository>();
        services.AddScoped<ICoursesRepository, CoursesDapperRepository>();
        services.AddScoped<ISeminarsRepository, SeminarsDapperRepository>();
        services.AddScoped<IWebinarsRepository, WebinarsDapperRepository>();

        // Genuinely-new Operational.list-view modules (previously miscategorized as
        // zero-Application/zero-Api stubs; scaffolded 2026-08-20 following this exact
        // EquipmentSafety/Webinars list-view convention -- brand-new *ReadModel tables +
        // usp_*_List procedures, no pre-existing SQL objects).
        services.AddScoped<IEnvironmentalMonitoringRepository, EnvironmentalMonitoringDapperRepository>();
        services.AddScoped<IWellnessRepository, WellnessDapperRepository>();
        services.AddScoped<IWorkforceSafetyRepository, WorkforceSafetyDapperRepository>();

        // Cross-cutting Operational repositories
        services.AddScoped<IOperationalBulkRepository, OperationalBulkDapperRepository>();
        services.AddScoped<IExportOperationalRepository, ExportOperationalDapperRepository>();

        return services;
    }
}
