// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.DependencyInjection
//  Layer:          Infrastructure
//  File:           MachineSafetyInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the MachineSafety Infrastructure
//                  layer (6 sub-component aggregate repositories, each implementing
//                  both its Domain write-repository interface and its Application read-model
//                  repository interface against the same Dapper repository class). Call
//                  <c>services.AddMachineSafetyInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by
//                  the shared InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  that file is out of scope for this pass; wiring the call site there is a
//                  required, tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.Repositories;
using PlantSafety.Infrastructure.MachineSafety.Persistence;
using PlantSafety.Infrastructure.MachineSafety.EmergencyStopDevices.Persistence;
using PlantSafety.Infrastructure.MachineSafety.LotoSystems.Persistence;
using PlantSafety.Infrastructure.MachineSafety.MachineGuardsBarriers.Persistence;
using PlantSafety.Infrastructure.MachineSafety.PresenceSensingDevices.Persistence;
using PlantSafety.Infrastructure.MachineSafety.SafetyValvesRelays.Persistence;
using PlantSafety.Infrastructure.MachineSafety.TwoHandControls.Persistence;

namespace PlantSafety.Infrastructure.MachineSafety.DependencyInjection;

/// <summary>Per-module DI entry point for the MachineSafety Infrastructure layer.</summary>
public static class MachineSafetyInfrastructureServiceRegistration
{
    /// <summary>Registers all MachineSafety Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddMachineSafetyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Interfaces.IEmergencyStopDevicesRepository, EmergencyStopDevicesDapperRepository>();
        services.AddScoped<PlantSafety.Application.MachineSafety.EmergencyStopDevices.Repositories.IEmergencyStopDevicesRepository, EmergencyStopDevicesDapperRepository>();
        services.AddScoped<PlantSafety.Domain.MachineSafety.LotoSystems.Interfaces.ILotoSystemsRepository, LotoSystemsDapperRepository>();
        services.AddScoped<PlantSafety.Application.MachineSafety.LotoSystems.Repositories.ILotoSystemsRepository, LotoSystemsDapperRepository>();
        services.AddScoped<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Interfaces.IMachineGuardsBarriersRepository, MachineGuardsBarriersDapperRepository>();
        services.AddScoped<PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Repositories.IMachineGuardsBarriersRepository, MachineGuardsBarriersDapperRepository>();
        services.AddScoped<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces.IPresenceSensingDevicesRepository, PresenceSensingDevicesDapperRepository>();
        services.AddScoped<PlantSafety.Application.MachineSafety.PresenceSensingDevices.Repositories.IPresenceSensingDevicesRepository, PresenceSensingDevicesDapperRepository>();
        services.AddScoped<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Interfaces.ISafetyValvesRelaysRepository, SafetyValvesRelaysDapperRepository>();
        services.AddScoped<PlantSafety.Application.MachineSafety.SafetyValvesRelays.Repositories.ISafetyValvesRelaysRepository, SafetyValvesRelaysDapperRepository>();
        services.AddScoped<PlantSafety.Domain.MachineSafety.TwoHandControls.Interfaces.ITwoHandControlsRepository, TwoHandControlsDapperRepository>();
        services.AddScoped<PlantSafety.Application.MachineSafety.TwoHandControls.Repositories.ITwoHandControlsRepository, TwoHandControlsDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<IMachineSafetyRepository, MachineSafetyRepository>();

        return services;
    }
}
