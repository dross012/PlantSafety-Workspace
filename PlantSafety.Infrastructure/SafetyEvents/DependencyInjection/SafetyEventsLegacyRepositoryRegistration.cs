// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DependencyInjection
//  Layer:          Infrastructure
//  File:           SafetyEventsLegacyRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the pre-existing SafetyEvents
//                  module repository interfaces (PlantSafety.Application.SafetyEvents.*.Repositories,
//                  PlantSafety.Application.SafetyEvents.Commands.ISafetyEventsBulkRepository) that
//                  were scaffolded with Dapper implementations under
//                  PlantSafety.Infrastructure\SafetyEvents\ but never wired into the DI container.
//                  This gap was one of the reasons ValidateOnBuild had to be disabled in
//                  PlantSafety.Api\Program.cs.
//
//                  This file covers all 9 SafetyEvents legacy repository interfaces: CrisisManagement,
//                  DrillsAndExercises, EmergencyManagement, IncidentManagement, NearMisses, Observation,
//                  RootCauseAnalysis, Export (root-level) and Commands.BulkUpdate. Every repository
//                  class listed below already fully implemented its interface; the underlying SQL
//                  objects (tables + usp_* stored procedures) had been scaffolded as .sql files under
//                  PlantSafety.Infrastructure\SafetyEvents\Sql\{Tables,Procedures}\ but never deployed
//                  to the live database — they were deployed as part of this pass. No new C#
//                  repository logic was required; only SQL deployment and DI wiring.
//
//                  NOTE: [SafetyEvents].[IObservationRepository] here refers to the
//                  PlantSafety.Application.SafetyEvents.Observation.Repositories.IObservationRepository
//                  interface (backed by ObservationItemsDapperRepository / usp_ObservationItems_*),
//                  which is distinct from the already-registered
//                  PlantSafety.Application.Features.SafetyEvents.Handlers.Observation.IObservationRepository
//                  (backed by ObservationDapperRepository, registered in the central
//                  InfrastructureServiceRegistration.cs under the "Features tree" section).
//
//                  Call <c>services.AddSafetyEventsLegacyRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per task
//                  constraints, that central file is intentionally not edited here). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.Commands;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;
using PlantSafety.Application.SafetyEvents.IncidentManagement.Repositories;
using PlantSafety.Application.SafetyEvents.NearMisses.Repositories;
using PlantSafety.Application.SafetyEvents.Observation.Repositories;
using PlantSafety.Application.SafetyEvents.Repositories;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Repositories;
using PlantSafety.Infrastructure.SafetyEvents.Commands.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.CrisisManagement.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.DrillsAndExercises.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.IncidentManagement.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.NearMisses.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.Observation.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.Persistence;
using PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence;

namespace PlantSafety.Infrastructure.SafetyEvents.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the 9 pre-existing SafetyEvents module repository interfaces
/// (CrisisManagement, DrillsAndExercises, EmergencyManagement, IncidentManagement, NearMisses,
/// Observation, RootCauseAnalysis, Export, Commands.BulkUpdate) that were scaffolded with Dapper
/// implementations but never registered in the DI container.
/// </summary>
public static class SafetyEventsLegacyRepositoryRegistration
{
    /// <summary>Registers this batch's 9 legacy SafetyEvents Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddSafetyEventsLegacyRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICrisisManagementRepository, CrisisManagementDapperRepository>();
        services.AddScoped<IDrillsAndExercisesRepository, DrillsAndExercisesDapperRepository>();
        services.AddScoped<IEmergencyManagementRepository, EmergencyManagementDapperRepository>();
        services.AddScoped<IIncidentManagementRepository, IncidentManagementDapperRepository>();
        services.AddScoped<INearMissesRepository, NearMissesDapperRepository>();
        services.AddScoped<IObservationRepository, ObservationItemsDapperRepository>();
        services.AddScoped<IRootCauseAnalysisRepository, RootCauseAnalysisDapperRepository>();
        services.AddScoped<IExportSafetyEventsRepository, ExportSafetyEventsDapperRepository>();
        services.AddScoped<ISafetyEventsBulkRepository, SafetyEventsBulkDapperRepository>();

        return services;
    }
}
