// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Registers AuditReport Application-layer services (the Domain-layer factory
//   and content builder). Renderer implementations (IAuditReportRenderer) are registered by the
//   Infrastructure layer's own DI extension, since they depend on concrete rendering libraries.
//   Self-contained extension method; wired into the composition root by the host application.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Domain.Auditing.AuditReport.Factories;
using PlantSafety.Domain.Auditing.AuditReport.Interfaces;
using PlantSafety.Domain.Auditing.AuditReport.Services;

namespace PlantSafety.Application.Auditing.AuditReport.DependencyInjection;

/// <summary>Extension methods to register AuditReport Application-layer services.</summary>
public static class AuditReportApplicationServiceRegistration
{
    /// <summary>Registers AuditReport-specific Application services.</summary>
    public static IServiceCollection AddAuditReportApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditReportFactory, AuditReportFactory>();
        services.AddSingleton<AuditReportContentBuilder>();

        return services;
    }
}
