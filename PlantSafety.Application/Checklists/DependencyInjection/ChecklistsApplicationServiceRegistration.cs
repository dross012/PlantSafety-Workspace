// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers Checklists module Application-layer services. Self-contained extension method; wired into the composition root by the host application. Not auto-linked into the shared ApplicationServiceRegistration entry point to avoid cross-agent merge collisions during the legacy-module scaffolding effort -- see module README.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Checklists.DependencyInjection;

/// <summary>
/// Extension methods to register Checklists Application-layer services.
/// </summary>
public static class ChecklistsApplicationServiceRegistration
{
    /// <summary>
    /// Registers Checklists-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddChecklistsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}