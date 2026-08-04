// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DependencyInjection
//  Layer:          Infrastructure
//  File:           LogisticsTransportSafetyInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the LogisticsTransportSafety
//                  Infrastructure layer (6 sub-component aggregate repositories, each implementing
//                  both its Domain write-repository interface and its Application read-model
//                  repository interface against the same Dapper repository class). Call
//                  <c>services.AddLogisticsTransportSafetyInfrastructure()</c> from the host
//                  startup alongside <c>services.AddInfrastructure(configuration)</c>.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.Repositories;
using PlantSafety.Infrastructure.LogisticsTransportSafety.Persistence;
using PlantSafety.Infrastructure.LogisticsTransportSafety.TransportVehicles.Persistence;
using PlantSafety.Infrastructure.LogisticsTransportSafety.Carriers.Persistence;
using PlantSafety.Infrastructure.LogisticsTransportSafety.DriverQualifications.Persistence;
using PlantSafety.Infrastructure.LogisticsTransportSafety.TransportTrips.Persistence;
using PlantSafety.Infrastructure.LogisticsTransportSafety.LoadSecurementChecks.Persistence;
using PlantSafety.Infrastructure.LogisticsTransportSafety.TransportIncidents.Persistence;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.DependencyInjection;

/// <summary>Per-module DI entry point for the LogisticsTransportSafety Infrastructure layer.</summary>
public static class LogisticsTransportSafetyInfrastructureServiceRegistration
{
    /// <summary>Registers all LogisticsTransportSafety Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddLogisticsTransportSafetyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces.ITransportVehicleRepository, TransportVehicleDapperRepository>();
        services.AddScoped<PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Repositories.ITransportVehicleRepository, TransportVehicleDapperRepository>();
        services.AddScoped<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces.ICarrierRepository, CarrierDapperRepository>();
        services.AddScoped<PlantSafety.Application.LogisticsTransportSafety.Carriers.Repositories.ICarrierRepository, CarrierDapperRepository>();
        services.AddScoped<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces.IDriverQualificationRepository, DriverQualificationDapperRepository>();
        services.AddScoped<PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Repositories.IDriverQualificationRepository, DriverQualificationDapperRepository>();
        services.AddScoped<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces.ITransportTripRepository, TransportTripDapperRepository>();
        services.AddScoped<PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Repositories.ITransportTripRepository, TransportTripDapperRepository>();
        services.AddScoped<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Interfaces.ILoadSecurementCheckRepository, LoadSecurementCheckDapperRepository>();
        services.AddScoped<PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Repositories.ILoadSecurementCheckRepository, LoadSecurementCheckDapperRepository>();
        services.AddScoped<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces.ITransportIncidentRepository, TransportIncidentDapperRepository>();
        services.AddScoped<PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Repositories.ITransportIncidentRepository, TransportIncidentDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<ILogisticsTransportSafetyRepository, LogisticsTransportSafetyRepository>();

        return services;
    }
}
