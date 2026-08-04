// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.DependencyInjection
//  Layer:          Infrastructure
//  File:           AutonomousVehicleSafetyInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the AutonomousVehicleSafety
//                  Infrastructure layer (5 sub-component aggregate repositories, each implementing
//                  both its Domain write-repository interface and its Application read-model
//                  repository interface against the same Dapper repository class). Call
//                  <c>services.AddAutonomousVehicleSafetyInfrastructure()</c> from
//                  InfrastructureServiceRegistration.AddInfrastructure(). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.Repositories;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.Persistence;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.AutonomousVehicleAsset.Persistence;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.OperatingGeofence.Persistence;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.SafetyCaseValidation.Persistence;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.HumanRobotInteractionEvent.Persistence;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.AutonomousVehicleIncident.Persistence;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.DependencyInjection;

/// <summary>Per-module DI entry point for the AutonomousVehicleSafety Infrastructure layer.</summary>
public static class AutonomousVehicleSafetyInfrastructureServiceRegistration
{
    /// <summary>Registers all AutonomousVehicleSafety Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddAutonomousVehicleSafetyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces.IAutonomousVehicleAssetRepository, AutonomousVehicleAssetDapperRepository>();
        services.AddScoped<PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Repositories.IAutonomousVehicleAssetRepository, AutonomousVehicleAssetDapperRepository>();
        services.AddScoped<PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Interfaces.IOperatingGeofenceRepository, OperatingGeofenceDapperRepository>();
        services.AddScoped<PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Repositories.IOperatingGeofenceRepository, OperatingGeofenceDapperRepository>();
        services.AddScoped<PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Interfaces.ISafetyCaseValidationRepository, SafetyCaseValidationDapperRepository>();
        services.AddScoped<PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Repositories.ISafetyCaseValidationRepository, SafetyCaseValidationDapperRepository>();
        services.AddScoped<PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Interfaces.IHumanRobotInteractionEventRepository, HumanRobotInteractionEventDapperRepository>();
        services.AddScoped<PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Repositories.IHumanRobotInteractionEventRepository, HumanRobotInteractionEventDapperRepository>();
        services.AddScoped<PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Interfaces.IAutonomousVehicleIncidentRepository, AutonomousVehicleIncidentDapperRepository>();
        services.AddScoped<PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Repositories.IAutonomousVehicleIncidentRepository, AutonomousVehicleIncidentDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<IAutonomousVehicleSafetyRepository, AutonomousVehicleSafetyRepository>();

        return services;
    }
}
