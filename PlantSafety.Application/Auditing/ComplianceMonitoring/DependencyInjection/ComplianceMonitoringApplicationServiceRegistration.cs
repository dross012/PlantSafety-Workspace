// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Registers Auditing.ComplianceMonitoring's SharedKernel
//   IReportSectionProvider<,> implementation(s), letting the ComplianceReport submodule's rollup
//   query resolve the PermissionUsage tenant-audit-stream section via DI instead of dispatching
//   PermissionUsage.Queries directly through ISender (Handbook Part IX §9.3.2). MediatR handlers
//   for the underlying Queries remain auto-registered via assembly scan; only this provider adapter
//   requires explicit registration. Self-contained extension method; MUST be invoked from
//   ApplicationServiceRegistration.AddApplicationServices -- see that file's call site.
// SchemaVersion: 1
// Additive-only evolution: new provider registrations may be added; no existing registration may
//   be removed.

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Contracts;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.DependencyInjection;

/// <summary>Registers Application-layer cross-module composition services for the Auditing.ComplianceMonitoring module.</summary>
public static class ComplianceMonitoringApplicationServiceRegistration
{
    /// <summary>Adds Auditing.ComplianceMonitoring's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation(s).</summary>
    public static IServiceCollection AddComplianceMonitoringApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IReportSectionProvider<TenantAuditStreamSectionRequest, TenantAuditStreamResult>, TenantAuditStreamSectionProvider>();

        return services;
    }
}
