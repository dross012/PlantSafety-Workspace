// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Registers every Operations.* submodule's SharedKernel
//   IReportSectionProvider<,> implementation. These let Operations.SystemHealth (and any future
//   composing submodule) resolve sibling submodules' read-model sections via DI instead of
//   dispatching their Commands/Queries directly through ISender (Handbook Part IX §9.3.2 "Modules
//   May Only Communicate via Public Contracts"). Self-contained extension method; wired into the
//   composition root via AddApplicationServices(). MediatR handlers and FluentValidation
//   validators for the underlying Queries remain auto-registered via assembly scan; only these
//   provider adapters require explicit registration.
// SchemaVersion: 1
// Additive-only evolution: new provider registrations may be added; no existing registration may
//   be removed.

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.Application.Operations.Archival.Contracts;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.AuditViewer.Contracts;
using PlantSafety.Application.Operations.Backup.Contracts;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Configuration.Contracts;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Monitoring.Contracts;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.SchemaGuard.Contracts;
using PlantSafety.Application.Operations.SchemaGuard.Models;

namespace PlantSafety.Application.Operations.DependencyInjection;

/// <summary>
/// Extension methods to register every Operations.* submodule's cross-module read-model
/// composition provider.
/// </summary>
public static class OperationsReadModelCompositionServiceRegistration
{
    /// <summary>
    /// Registers every Operations.* <see cref="IReportSectionProvider{TRequest, TSection}"/>
    /// implementation used by the Operations.SystemHealth rollup report.
    /// </summary>
    public static IServiceCollection AddOperationsReadModelCompositionServices(this IServiceCollection services)
    {
        services.AddScoped<IReportSectionProvider<NoSectionRequest, IReadOnlyList<BackupHealthDto>>, BackupHealthSectionProvider>();
        services.AddScoped<IReportSectionProvider<NoSectionRequest, IReadOnlyList<SchemaDatabaseHealthDto>>, SchemaHealthSectionProvider>();
        services.AddScoped<IReportSectionProvider<NoSectionRequest, DatabaseHealthReportDto>, DatabaseHealthSectionProvider>();
        services.AddScoped<IReportSectionProvider<ErrorLogSectionRequest, IReadOnlyList<ErrorLogItemDto>>, ErrorLogSectionProvider>();
        services.AddScoped<IReportSectionProvider<QueryPerformanceSectionRequest, IReadOnlyList<QueryPerformanceItemDto>>, QueryPerformanceSectionProvider>();
        services.AddScoped<IReportSectionProvider<PurgeStatusSectionRequest, IReadOnlyList<ArchivalJobDto>>, PurgeStatusSectionProvider>();
        services.AddScoped<IReportSectionProvider<RecentAuditActivitySectionRequest, int>, RecentAuditActivitySectionProvider>();
        services.AddScoped<IReportSectionProvider<NoSectionRequest, IReadOnlyList<FeatureFlagDto>>, FeatureFlagsSectionProvider>();

        return services;
    }
}
