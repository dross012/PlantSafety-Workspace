// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers EmployeeSafety module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.EmployeeSafety.DependencyInjection;

/// <summary>
/// Extension methods to register EmployeeSafety Application-layer services.
/// </summary>
public static class EmployeeSafetyApplicationServiceRegistration
{
    /// <summary>
    /// Registers EmployeeSafety-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddEmployeeSafetyApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}