// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Registers AuditTimeline Application-layer services (the Domain-layer
//   composition service and factory). Self-contained extension method; wired into the
//   composition root by the host application. MediatR handlers and FluentValidation validators
//   are auto-registered via assembly scan.
//   2026-08-03: also registers AuditTimelineSectionProvider -- the SharedKernel
//   IReportSectionProvider<,> implementation that lets sibling submodules (e.g. AuditReport)
//   compose the AuditTimeline read model via DI instead of dispatching
//   GetAuditTimelineForEntityQuery directly through ISender (Handbook Part IX §9.3.2).
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Domain.Auditing.AuditTimeline.Factories;
using PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;
using PlantSafety.Domain.Auditing.AuditTimeline.Services;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.Application.Auditing.AuditTimeline.Contracts;
using PlantSafety.Application.Auditing.AuditTimeline.Models;

namespace PlantSafety.Application.Auditing.AuditTimeline.DependencyInjection;

/// <summary>Extension methods to register AuditTimeline Application-layer services.</summary>
public static class AuditTimelineApplicationServiceRegistration
{
    /// <summary>Registers AuditTimeline-specific Application services.</summary>
    public static IServiceCollection AddAuditTimelineApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IConcurrencyConflictFactory, ConcurrencyConflictFactory>();
        services.AddSingleton<AuditTimelineCompositionService>();

        services.AddScoped<IReportSectionProvider<AuditTimelineSectionRequest, AuditTimelineDto>, AuditTimelineSectionProvider>();

        return services;
    }
}
