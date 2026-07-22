// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.DependencyInjection
//  Layer:          Infrastructure
//  File:           ComplianceLegacyRepositoryRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the pre-existing Compliance
//                  module repository interfaces (PlantSafety.Application.Compliance.*.Repositories)
//                  that were scaffolded with Dapper implementations under
//                  PlantSafety.Infrastructure\Compliance\ but never wired into the DI container.
//                  This gap was one of the reasons ValidateOnBuild had to be disabled in
//                  PlantSafety.Api\Program.cs.
//
//                  This file covers a batch of 30 Compliance repository interfaces spanning the
//                  Policy, Procedures, RegulatoryCalendar (Deadlines/Inspections/Renewals),
//                  RegulatoryRequirements, Tracking and root-level Export sub-trees. Every
//                  repository class listed below already fully implements its interface. Of the
//                  30, 26 Dapper repository classes (plus the pre-existing ExportComplianceDapperRepository)
//                  were already present and correct; the remaining underlying SQL objects
//                  ([Compliance].[Policy*], [Compliance].[Procedures*], [Compliance].[Deadlines*],
//                  [Compliance].[Inspections*], [Compliance].[Renewals*], [Compliance].[RegulatoryRequirements],
//                  [Compliance].[Tracking], [Compliance].[ComplianceExportItems] tables and their
//                  usp_*_List stored procedures) had been scaffolded as .sql files under
//                  PlantSafety.Infrastructure\Compliance\Sql\{Tables,Procedures}\ but never deployed
//                  to the live database — they were deployed as part of this pass. No new C#
//                  repository logic was required; only SQL deployment and DI wiring.
//
//                  A sibling pass covers the other 31 pre-existing Compliance repositories
//                  (Auditing, Certification, ManagementOfChange, Permits, Commands, Obligations,
//                  Policy.Approvals/ChemicalHandling) — those are NOT registered here to avoid
//                  duplicate registrations; see that pass's own registration additions (expected
//                  additively in this same file or a sibling one).
//
//                  Call <c>services.AddComplianceLegacyRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Contractor.Repositories;
using PlantSafety.Application.Compliance.Policy.Documents.Repositories;
using PlantSafety.Application.Compliance.Policy.Emergency.Repositories;
using PlantSafety.Application.Compliance.Policy.Environmental.Repositories;
using PlantSafety.Application.Compliance.Policy.HR.Repositories;
using PlantSafety.Application.Compliance.Policy.PPE.Repositories;
using PlantSafety.Application.Compliance.Policy.Revisions.Repositories;
using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Repositories;
using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Repositories;
using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Repositories;
using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Repositories;
using PlantSafety.Application.Compliance.Procedures.Inspection.Repositories;
using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Repositories;
using PlantSafety.Application.Compliance.Procedures.Maintenance.Repositories;
using PlantSafety.Application.Compliance.Procedures.SOPs.Repositories;
using PlantSafety.Application.Compliance.Procedures.Safety.Repositories;
using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Repositories;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Repositories;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Repositories;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Repositories;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Repositories;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Repositories;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.Training.Repositories;
using PlantSafety.Application.Compliance.RegulatoryRequirements.Repositories;
using PlantSafety.Application.Compliance.Repositories;
using PlantSafety.Application.Compliance.Tracking.Repositories;
using PlantSafety.Infrastructure.Compliance.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.Contractor.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.Documents.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.Emergency.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.Environmental.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.Equipment.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.HR.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.PPE.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.Revisions.Persistence;
using PlantSafety.Infrastructure.Compliance.Policy.SafetyPolicies.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.EmergencyProcedure.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.HazMatProcedures.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.IncidentResponse.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.Inspection.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.LockoutTagoutProcedure.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.Maintenance.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.SOPs.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.Safety.Persistence;
using PlantSafety.Infrastructure.Compliance.Procedures.WorkInstructions.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Deadlines.EPA.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.DOT.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.Equipment.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.OSHA.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.DOT.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.ISO.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.Training.Persistence;
using PlantSafety.Infrastructure.Compliance.RegulatoryRequirements.Persistence;
using PlantSafety.Infrastructure.Compliance.Tracking.Persistence;

namespace PlantSafety.Infrastructure.Compliance.DependencyInjection;

/// <summary>
/// Per-module DI entry point for a batch of 30 pre-existing Compliance module repository
/// interfaces (Policy, Procedures, RegulatoryCalendar, RegulatoryRequirements, Tracking, Export)
/// that were scaffolded with Dapper implementations but never registered in the DI container.
/// </summary>
public static class ComplianceLegacyRepositoryRegistration
{
    /// <summary>Registers this batch's 30 legacy Compliance Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddComplianceLegacyRepositories(this IServiceCollection services)
    {
        // Policy subsystem
        services.AddScoped<IContractorRepository, ContractorDapperRepository>();
        services.AddScoped<IDocumentsRepository, DocumentsDapperRepository>();
        services.AddScoped<IEmergencyRepository, EmergencyDapperRepository>();
        services.AddScoped<IEnvironmentalRepository, EnvironmentalDapperRepository>();
        services.AddScoped<
            Application.Compliance.Policy.Equipment.Repositories.IEquipmentRepository,
            PolicyEquipmentDapperRepository>();
        services.AddScoped<IHRRepository, HRDapperRepository>();
        services.AddScoped<IPPERepository, PPEDapperRepository>();
        services.AddScoped<IRevisionsRepository, RevisionsDapperRepository>();
        services.AddScoped<ISafetyPoliciesRepository, SafetyPoliciesDapperRepository>();

        // Procedures subsystem
        services.AddScoped<IEmergencyProcedureRepository, EmergencyProcedureDapperRepository>();
        services.AddScoped<IHazMatProceduresRepository, HazMatProceduresDapperRepository>();
        services.AddScoped<IIncidentResponseRepository, IncidentResponseDapperRepository>();
        services.AddScoped<IInspectionRepository, InspectionDapperRepository>();
        services.AddScoped<ILockoutTagoutProcedureRepository, LockoutTagoutProcedureDapperRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceDapperRepository>();
        services.AddScoped<ISOPsRepository, SOPsDapperRepository>();
        services.AddScoped<ISafetyRepository, SafetyDapperRepository>();
        services.AddScoped<IWorkInstructionsRepository, WorkInstructionsDapperRepository>();

        // RegulatoryCalendar.Deadlines subsystem
        services.AddScoped<IAnnualAuditRepository, AnnualAuditDapperRepository>();
        services.AddScoped<IEPARepository, EPADapperRepository>();

        // RegulatoryCalendar.Inspections subsystem
        services.AddScoped<
            Application.Compliance.RegulatoryCalendar.Inspections.DOT.Repositories.IDOTRepository,
            InspectionsDOTDapperRepository>();
        services.AddScoped<IEnvironmentalSamplingRepository, EnvironmentalSamplingDapperRepository>();
        services.AddScoped<
            Application.Compliance.RegulatoryCalendar.Inspections.Equipment.Repositories.IEquipmentRepository,
            InspectionsEquipmentDapperRepository>();
        services.AddScoped<IOSHARepository, OSHADapperRepository>();

        // RegulatoryCalendar.Renewals subsystem
        services.AddScoped<
            Application.Compliance.RegulatoryCalendar.Renewals.DOT.Repositories.IDOTRepository,
            PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.DOT.Persistence.RenewalsDOTDapperRepository>();
        services.AddScoped<IISORepository, ISODapperRepository>();
        services.AddScoped<ITrainingRepository, TrainingDapperRepository>();

        // Root-level Compliance subsystems
        services.AddScoped<IRegulatoryRequirementsRepository, RegulatoryRequirementsDapperRepository>();
        services.AddScoped<IExportComplianceRepository, ExportComplianceDapperRepository>();
        services.AddScoped<ITrackingRepository, TrackingDapperRepository>();

        return services;
    }
}
