// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           AuditReportInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the AuditReport Infrastructure-layer
//                  repository, content-store, and renderer implementations. NOT wired into the
//                  shared InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditReport.Interfaces;
using PlantSafety.Application.Auditing.AuditReport.Abstractions;
using PlantSafety.Application.Auditing.AuditReport.Repositories;
using PlantSafety.Infrastructure.Auditing.AuditReport.Persistence;
using PlantSafety.Infrastructure.Auditing.AuditReport.Rendering;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the AuditReport submodule.</summary>
public static class AuditReportInfrastructureServiceRegistration
{
    /// <summary>Adds AuditReport Dapper repositories, blob content store, and the three format renderers.</summary>
    public static IServiceCollection AddAuditReportInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAuditReportRepository, AuditReportDapperRepository>();
        services.AddScoped<IAuditReportReadRepository, AuditReportReadDapperRepository>();
        services.AddScoped<IAuditReportContentStore, AuditReportBlobContentStore>();

        services.AddScoped<IAuditReportRenderer, QuestPdfAuditReportRenderer>();
        services.AddScoped<IAuditReportRenderer, CsvAuditReportRenderer>();
        services.AddScoped<IAuditReportRenderer, JsonAuditReportRenderer>();

        return services;
    }
}
