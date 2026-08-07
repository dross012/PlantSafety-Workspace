// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.DependencyInjection
//  Layer:          Infrastructure
//  File:           AuditingInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the Auditing Infrastructure
//                  layer (2 Domain aggregate repositories + 8 Application read repositories).
//                  Deliberately separate from the pre-existing Compliance.Auditing module's
//                  registrations (see PlantSafety.Infrastructure\Compliance\Auditing). Call
//                  <c>services.AddAuditingInfrastructure()</c> from the host startup alongside
//                  <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the shared
//                  InfrastructureServiceRegistration.AddInfrastructure() yet -- that file is
//                  out of scope for this pass; wiring the call site there is a required,
//                  tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.Operations.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Repositories;
using PlantSafety.Application.Auditing.AuditEngagement.Systems.Repositories;
using PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;
using PlantSafety.Domain.Auditing.AuditFinding.Interfaces;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.Efficiency.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.FacilityAssessment.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.Inspecting.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.Operations.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.Procedures.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.QualityControl.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.RegulatoryCompliance.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditEngagement.Systems.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditFinding.Persistence;
using PlantSafety.Application.Auditing.AuditFinding.OshaCoverage.Repositories;
using PlantSafety.Infrastructure.Auditing.AuditFinding.OshaCoverage.Persistence;
using PlantSafety.Application.Auditing.AuditFinding.NfpaCoverage.Repositories;
using PlantSafety.Infrastructure.Auditing.AuditFinding.NfpaCoverage.Persistence;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;
using PlantSafety.Infrastructure.Auditing.ComplianceMonitoring.PermissionUsage.Persistence;
using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Repositories;
using PlantSafety.Infrastructure.Auditing.ComplianceMonitoring.DataLineage.Persistence;

namespace PlantSafety.Infrastructure.Auditing.DependencyInjection;

/// <summary>Per-module DI entry point for the Auditing Infrastructure layer.</summary>
public static class AuditingInfrastructureServiceRegistration
{
    /// <summary>Registers all Auditing Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddAuditingInfrastructure(this IServiceCollection services)
    {
        // Domain aggregate repositories
        services.AddScoped<IAuditEngagementRepository, AuditEngagementDapperRepository>();
        services.AddScoped<Domain.Auditing.AuditFinding.Interfaces.IAuditFindingRepository, AuditFindingDapperRepository>();
        services.AddScoped<IAuditFindingOshaCoverageRepository, AuditFindingOshaCoverageDapperRepository>();
        services.AddScoped<IAuditFindingNfpaCoverageRepository, AuditFindingNfpaCoverageDapperRepository>();

        // Phase 6 §5.4 Compliance Monitoring (2026-08-03): access-log/permission-usage capture
        // (fed by AuthorizationBehavior) and cross-module data-lineage reconstruction (composed
        // from the EXISTING Outbox + DeadLetterEvents infrastructure, not a new parallel system).
        services.AddScoped<IPermissionUsageLogRepository, PermissionUsageLogDapperRepository>();
        services.AddScoped<IDataLineageRepository, DataLineageDapperRepository>();

        // Phase 7 §5.4 Audit & Logging (2026-08-03, Security & Compliance Agent): read-side
        // companion to IPermissionUsageLogRepository backing GetTenantAuditStreamQuery -- the
        // tenant-scoped access-log audit stream.
        services.AddScoped<IPermissionUsageLogReadRepository, PermissionUsageLogReadDapperRepository>();

        // Application discipline read repositories
        services.AddScoped<IEfficiencyRepository, EfficiencyDapperRepository>();
        services.AddScoped<IFacilityAssessmentRepository, FacilityAssessmentDapperRepository>();
        services.AddScoped<IInspectingRepository, InspectingDapperRepository>();
        services.AddScoped<IOperationsRepository, OperationsDapperRepository>();
        services.AddScoped<IProceduresRepository, ProceduresDapperRepository>();
        services.AddScoped<IQualityControlRepository, QualityControlDapperRepository>();
        services.AddScoped<IRegulatoryComplianceRepository, RegulatoryComplianceDapperRepository>();
        services.AddScoped<ISystemsRepository, SystemsDapperRepository>();

        return services;
    }
}
