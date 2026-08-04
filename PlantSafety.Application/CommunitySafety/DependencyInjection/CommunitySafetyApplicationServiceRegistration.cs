// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers CommunitySafety module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.CommunitySafety.DependencyInjection;

/// <summary>
/// Extension methods to register CommunitySafety Application-layer services.
/// </summary>
public static class CommunitySafetyApplicationServiceRegistration
{
    /// <summary>
    /// Registers CommunitySafety-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddCommunitySafetyApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
