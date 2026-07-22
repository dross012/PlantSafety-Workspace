// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers ContractorSafety module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.ContractorSafety.DependencyInjection;

/// <summary>
/// Extension methods to register ContractorSafety Application-layer services.
/// </summary>
public static class ContractorSafetyApplicationServiceRegistration
{
    /// <summary>
    /// Registers ContractorSafety-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddContractorSafetyApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}