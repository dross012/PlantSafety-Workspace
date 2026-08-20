// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.DependencyInjection
//  Layer:          Infrastructure
//  File:           ComplianceLegacyRepositoryRegistrationBatch2.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the second batch of 31
//                   pre-existing Compliance module repository interfaces (Auditing,
//                   Certification, ManagementOfChange, Obligations, Permits,
//                   Policy.Approvals, Policy.ChemicalHandling) that were scaffolded with
//                   Dapper implementations under PlantSafety.Infrastructure\Compliance\
//                   but never wired into the DI container.
//
//                   All 31 Dapper repository classes already existed and correctly
//                   implemented their interfaces. The underlying gap was purely
//                   deployment: dedicated per-sub-type Table and Stored Procedure .sql
//                   scripts already existed under
//                   PlantSafety.Infrastructure\Compliance\Sql\{Tables,Procedures}\
//                   (one CREATE TABLE + one usp_*_Get* per interface, each shaped to
//                   match its DTO exactly) but had never been executed against the live
//                   database. All 31 Table scripts and all 31 Procedure scripts were
//                   deployed to localhost\PLANTSAFETY / PlantSafety.CoreDB as part of
//                   this pass. No new SQL objects were authored from scratch; the
//                   pre-existing scaffolded scripts were used as-is.
//
//                   NOTE: An earlier iteration of this pass (before the dedicated
//                   per-sub-type Table scripts were discovered) deployed a stopgap set of
//                   usp_* procedures that instead queried the shared generic aggregate
//                   tables ([Compliance].[Auditing]/[Certification]/[ManagementOfChange]/
//                   [Permits]/[Policy]) with best-effort column aliasing. Those procs have
//                   since been superseded (CREATE OR ALTER) by the correct, purpose-built
//                   procedures from Sql\Procedures\ that query the dedicated per-sub-type
//                   tables instead. No repository C# code needed to change either way,
//                   since the target object name never changed.
//
//                   All dedicated tables are freshly created and currently empty (0 rows)
//                   — no write-side (Create/Update) command handlers populate them yet.
//                   That is a separate, tracked gap outside this batch's scope.
//
//                   Call <c>services.AddComplianceLegacyRepositoriesBatch2()</c> from host
//                   startup alongside <c>services.AddInfrastructure(configuration)</c> and
//                   the sibling <c>AddComplianceLegacyRepositories()</c> call. NOT invoked
//                   by the shared InfrastructureServiceRegistration.AddInfrastructure() yet
//                   -- wiring the call site there is a required, tracked follow-up for a
//                   human maintainer (central file intentionally not edited here to avoid
//                   concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.AuditFindings.Repositories;
using PlantSafety.Application.Compliance.Auditing.CAPATriggers.Repositories;
using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Repositories;
using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Repositories;
using PlantSafety.Application.Compliance.Certification.Contractor.Repositories;
using PlantSafety.Application.Compliance.Certification.DOT.Repositories;
using PlantSafety.Application.Compliance.Certification.EPA.Repositories;
using PlantSafety.Application.Compliance.Certification.ISO14001.Repositories;
using PlantSafety.Application.Compliance.Certification.ISO45001.Repositories;
using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Repositories;
using PlantSafety.Application.Compliance.Certification.OSHACertifications.Repositories;
using PlantSafety.Application.Compliance.Certification.Training.Repositories;
using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Repositories;
using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Repositories;
using PlantSafety.Application.Compliance.Obligations.Repositories;
using PlantSafety.Application.Compliance.Permits.ChemicalHandling.Repositories;
using PlantSafety.Application.Compliance.Permits.ColdWork.Repositories;
using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Repositories;
using PlantSafety.Application.Compliance.Permits.Electrical.Repositories;
using PlantSafety.Application.Compliance.Permits.Excavation.Repositories;
using PlantSafety.Application.Compliance.Permits.HotTap.Repositories;
using PlantSafety.Application.Compliance.Permits.HotWork.Repositories;
using PlantSafety.Application.Compliance.Permits.Lifting.Repositories;
using PlantSafety.Application.Compliance.Permits.LineBreak.Repositories;
using PlantSafety.Application.Compliance.Permits.LockoutTagout.Repositories;
using PlantSafety.Application.Compliance.Permits.PressureTesting.Repositories;
using PlantSafety.Application.Compliance.Permits.Radiography.Repositories;
using PlantSafety.Application.Compliance.Permits.WorkatHeight.Repositories;
using PlantSafety.Infrastructure.Compliance.Auditing.AuditFindings.Persistence;
using PlantSafety.Infrastructure.Compliance.Auditing.CAPATriggers.Persistence;
using PlantSafety.Infrastructure.Compliance.Auditing.ComplianceAudit.Persistence;
using PlantSafety.Infrastructure.Compliance.Auditing.InternalSafetyAudit.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.Contractor.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.DOT.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.EPA.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.ISO14001.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.ISO45001.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.IndustrySpecific.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.OSHACertifications.Persistence;
using PlantSafety.Infrastructure.Compliance.Certification.Training.Persistence;
using PlantSafety.Infrastructure.Compliance.ManagementOfChange.MOCWorkflows.Persistence;
using PlantSafety.Infrastructure.Compliance.ManagementOfChange.RiskAssessments.Persistence;
using PlantSafety.Infrastructure.Compliance.Obligations.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.ChemicalHandling.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.ColdWork.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.ConfinedSpace.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.Electrical.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.Excavation.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.HotTap.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.HotWork.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.Lifting.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.LineBreak.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.LockoutTagout.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.PressureTesting.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.Radiography.Persistence;
using PlantSafety.Infrastructure.Compliance.Permits.WorkatHeight.Persistence;

namespace PlantSafety.Infrastructure.Compliance.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the second batch of 31 pre-existing Compliance module
/// repository interfaces (Auditing, Certification, ManagementOfChange, Obligations,
/// Permits, Policy.Approvals, Policy.ChemicalHandling).
/// </summary>
public static class ComplianceLegacyRepositoryRegistrationBatch2
{
    /// <summary>Registers this batch's 31 legacy Compliance Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddComplianceLegacyRepositoriesBatch2(this IServiceCollection services)
    {
        // Auditing subsystem
        services.AddScoped<IAuditFindingsRepository, AuditFindingsDapperRepository>();
        services.AddScoped<ICAPATriggersRepository, CAPATriggersDapperRepository>();
        services.AddScoped<IComplianceAuditRepository, ComplianceAuditDapperRepository>();
        services.AddScoped<IInternalSafetyAuditRepository, InternalSafetyAuditDapperRepository>();

        // Certification subsystem
        services.AddScoped<IContractorRepository, ContractorDapperRepository>();
        services.AddScoped<IDOTRepository, DOTDapperRepository>();
        services.AddScoped<IEPARepository, EPADapperRepository>();
        services.AddScoped<IISO14001Repository, ISO14001DapperRepository>();
        services.AddScoped<IISO45001Repository, ISO45001DapperRepository>();
        services.AddScoped<IIndustrySpecificRepository, IndustrySpecificDapperRepository>();
        services.AddScoped<IOSHACertificationsRepository, OSHACertificationsDapperRepository>();
        services.AddScoped<ITrainingRepository, TrainingDapperRepository>();

        // ManagementOfChange subsystem
        services.AddScoped<
            Application.Compliance.ManagementOfChange.Approvals.Repositories.IApprovalsRepository,
            PlantSafety.Infrastructure.Compliance.ManagementOfChange.Approvals.Persistence.ApprovalsDapperRepository>();
        services.AddScoped<IMOCWorkflowsRepository, MOCWorkflowsDapperRepository>();
        services.AddScoped<IRiskAssessmentsRepository, RiskAssessmentsDapperRepository>();

        // Obligations subsystem
        services.AddScoped<IObligationsRepository, ObligationsDapperRepository>();

        // Permits subsystem
        services.AddScoped<IChemicalHandlingRepository, ChemicalHandlingDapperRepository>();
        services.AddScoped<IColdWorkRepository, ColdWorkDapperRepository>();
        services.AddScoped<IConfinedSpaceRepository, ConfinedSpaceDapperRepository>();
        services.AddScoped<IElectricalRepository, ElectricalDapperRepository>();
        services.AddScoped<IExcavationRepository, ExcavationDapperRepository>();
        services.AddScoped<IHotTapRepository, HotTapDapperRepository>();
        services.AddScoped<IHotWorkRepository, HotWorkDapperRepository>();
        services.AddScoped<ILiftingRepository, LiftingDapperRepository>();
        services.AddScoped<ILineBreakRepository, LineBreakDapperRepository>();
        services.AddScoped<ILockoutTagoutRepository, LockoutTagoutDapperRepository>();
        services.AddScoped<IPressureTestingRepository, PressureTestingDapperRepository>();
        services.AddScoped<IRadiographyRepository, RadiographyDapperRepository>();
        services.AddScoped<IWorkatHeightRepository, WorkatHeightDapperRepository>();

        // Policy subsystem (Approvals / ChemicalHandling only — this batch's scope)
        services.AddScoped<
            Application.Compliance.Policy.Approvals.Repositories.IApprovalsRepository,
            PlantSafety.Infrastructure.Compliance.Policy.Approvals.Persistence.ApprovalsDapperRepository>();
        services.AddScoped<
            Application.Compliance.Policy.ChemicalHandling.Repositories.IChemicalHandlingRepository,
            PlantSafety.Infrastructure.Compliance.Policy.ChemicalHandling.Persistence.ChemicalHandlingDapperRepository>();

        return services;
    }
}
