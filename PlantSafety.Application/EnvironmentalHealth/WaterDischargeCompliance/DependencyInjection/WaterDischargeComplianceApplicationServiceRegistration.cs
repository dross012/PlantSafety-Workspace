// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers WaterDischargeCompliance sub-domain Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DependencyInjection;

/// <summary>
/// Extension methods to register WaterDischargeCompliance Application-layer services.
/// </summary>
public static class WaterDischargeComplianceApplicationServiceRegistration
{
    /// <summary>
    /// Registers WaterDischargeCompliance-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddWaterDischargeComplianceApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}