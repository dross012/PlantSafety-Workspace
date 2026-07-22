// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.DependencyInjection
//  Layer:          Infrastructure
//  File:           IntegrationsInfrastructureServiceRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the pre-existing Integrations
//                  module repository interfaces (PlantSafety.Application.Integrations.*.Repositories,
//                  PlantSafety.Application.Integrations.Commands.IInboundWebhookRepository) that
//                  had no Infrastructure implementation at all -- unlike other legacy modules in
//                  this pass, Integrations had zero SQL objects deployed (0 tables, 0 procedures
//                  under the [Integrations] schema) before this pass. This gap was one of the
//                  reasons ValidateOnBuild had to be disabled in PlantSafety.Api\Program.cs.
//
//                  This file covers all 14 Integrations repository interfaces: Analytics, Analyzers,
//                  Auditing, Documents, Email (root-level), Export, Import, IoT,
//                  Notifications.Email, Notifications.Internal, Notifications.Texting, Reporting,
//                  Texting (root-level), and Commands.IInboundWebhookRepository. For every one of
//                  the 14: a new [Integrations].[*Items] (or [InboundWebhookEvents]) table was
//                  created under PlantSafety.Infrastructure\Integrations\Sql\Tables\, a matching
//                  usp_* stored procedure was created under
//                  PlantSafety.Infrastructure\Integrations\Sql\Procedures\, both were deployed to
//                  the live database, and a new Dapper repository class was written and is
//                  registered below.
//
//                  NOTE on interface-name collisions: this module declares two distinct
//                  IEmailRepository interfaces (root-level Integrations.Email vs
//                  Integrations.Notifications.Email) and two distinct ITextingRepository
//                  interfaces (root-level Integrations.Texting vs Integrations.Notifications.Texting).
//                  Each is registered below using its fully-qualified interface name to avoid any
//                  ambiguity, backed by a correspondingly distinctly-named Dapper repository class
//                  (EmailDapperRepository/NotificationsEmailDapperRepository,
//                  TextingDapperRepository/NotificationsTextingDapperRepository).
//
//                  Call <c>services.AddIntegrationsInfrastructure()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Commands;
using PlantSafety.Infrastructure.Integrations.Analytics.Persistence;
using PlantSafety.Infrastructure.Integrations.Analyzers.Persistence;
using PlantSafety.Infrastructure.Integrations.Auditing.Persistence;
using PlantSafety.Infrastructure.Integrations.Commands.Persistence;
using PlantSafety.Infrastructure.Integrations.Documents.Persistence;
using PlantSafety.Infrastructure.Integrations.Export.Persistence;
using PlantSafety.Infrastructure.Integrations.Import.Persistence;
using PlantSafety.Infrastructure.Integrations.IoT.Persistence;
using PlantSafety.Infrastructure.Integrations.Notifications.Email.Persistence;
using PlantSafety.Infrastructure.Integrations.Notifications.Internal.Persistence;
using PlantSafety.Infrastructure.Integrations.Reporting.Persistence;

namespace PlantSafety.Infrastructure.Integrations.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the 14 pre-existing Integrations module repository interfaces
/// that had no SQL objects or Infrastructure implementations at all before this pass.
/// </summary>
public static class IntegrationsInfrastructureServiceRegistration
{
    /// <summary>Registers all 14 Integrations Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddIntegrationsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<
            Application.Integrations.Analytics.Repositories.IAnalyticsRepository,
            AnalyticsDapperRepository>();
        services.AddScoped<
            Application.Integrations.Analyzers.Repositories.IAnalyzersRepository,
            AnalyzersDapperRepository>();
        services.AddScoped<
            Application.Integrations.Auditing.Repositories.IAuditingRepository,
            AuditingDapperRepository>();
        services.AddScoped<IInboundWebhookRepository, InboundWebhookDapperRepository>();
        services.AddScoped<
            Application.Integrations.Documents.Repositories.IDocumentsRepository,
            DocumentsDapperRepository>();

        // Root-level Email vs Notifications.Email -- same simple interface name, different
        // namespaces. Registered fully qualified to avoid any ambiguity.
        services.AddScoped<
            Application.Integrations.Email.Repositories.IEmailRepository,
            Integrations.Email.Persistence.EmailDapperRepository>();
        services.AddScoped<
            Application.Integrations.Notifications.Email.Repositories.IEmailRepository,
            NotificationsEmailDapperRepository>();

        services.AddScoped<
            Application.Integrations.Export.Repositories.IExportRepository,
            ExportDapperRepository>();
        services.AddScoped<
            Application.Integrations.Import.Repositories.IImportRepository,
            ImportDapperRepository>();
        services.AddScoped<
            Application.Integrations.IoT.Repositories.IIoTRepository,
            IoTDapperRepository>();
        services.AddScoped<
            Application.Integrations.Notifications.Internal.Repositories.IInternalRepository,
            InternalDapperRepository>();
        services.AddScoped<
            Application.Integrations.Reporting.Repositories.IReportingRepository,
            ReportingDapperRepository>();

        // Root-level Texting vs Notifications.Texting -- same simple interface name, different
        // namespaces. Registered fully qualified to avoid any ambiguity.
        services.AddScoped<
            Application.Integrations.Texting.Repositories.ITextingRepository,
            Integrations.Texting.Persistence.TextingDapperRepository>();
        services.AddScoped<
            Application.Integrations.Notifications.Texting.Repositories.ITextingRepository,
            Integrations.Notifications.Texting.Persistence.NotificationsTextingDapperRepository>();

        return services;
    }
}
