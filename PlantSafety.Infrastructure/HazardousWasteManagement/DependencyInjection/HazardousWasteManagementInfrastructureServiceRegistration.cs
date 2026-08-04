// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DependencyInjection
//  Layer:          Infrastructure
//  File:           HazardousWasteManagementInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Per-module DI registration entry point for the HazardousWasteManagement
//                  Infrastructure layer (7 aggregate repositories, each implementing both its
//                  Domain write-repository interface and, where a read-model exists, its
//                  Application read-model repository interface against the same Dapper
//                  repository class). Call <c>services.AddHazardousWasteManagementInfrastructure()</c>
//                  from the host startup alongside <c>services.AddInfrastructure(configuration)</c>.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.HazardousWasteManagement.WasteStreams.Persistence;
using PlantSafety.Infrastructure.HazardousWasteManagement.WasteContainers.Persistence;
using PlantSafety.Infrastructure.HazardousWasteManagement.AccumulationAreas.Persistence;
using PlantSafety.Infrastructure.HazardousWasteManagement.Manifests.Persistence;
using PlantSafety.Infrastructure.HazardousWasteManagement.Transporters.Persistence;
using PlantSafety.Infrastructure.HazardousWasteManagement.TsdfFacilities.Persistence;
using PlantSafety.Infrastructure.HazardousWasteManagement.DisposalCertificates.Persistence;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.DependencyInjection;

/// <summary>Per-module DI entry point for the HazardousWasteManagement Infrastructure layer.</summary>
public static class HazardousWasteManagementInfrastructureServiceRegistration
{
    /// <summary>Registers all HazardousWasteManagement Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddHazardousWasteManagementInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces.IWasteStreamRepository, WasteStreamDapperRepository>();
        services.AddScoped<PlantSafety.Application.HazardousWasteManagement.WasteStreams.Repositories.IWasteStreamRepository, WasteStreamDapperRepository>();

        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces.IWasteContainerRepository, WasteContainerDapperRepository>();
        services.AddScoped<PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories.IWasteContainerRepository, WasteContainerDapperRepository>();

        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Interfaces.IAccumulationAreaRepository, AccumulationAreaDapperRepository>();
        services.AddScoped<PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Repositories.IAccumulationAreaRepository, AccumulationAreaDapperRepository>();

        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces.IHazardousWasteManifestRepository, HazardousWasteManifestDapperRepository>();
        services.AddScoped<PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories.IHazardousWasteManifestRepository, HazardousWasteManifestDapperRepository>();

        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.Transporters.Interfaces.ITransporterRepository, TransporterDapperRepository>();

        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Interfaces.ITsdfFacilityRepository, TsdfFacilityDapperRepository>();

        services.AddScoped<PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces.IDisposalCertificateRepository, DisposalCertificateDapperRepository>();
        services.AddScoped<PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Repositories.IDisposalCertificateRepository, DisposalCertificateDapperRepository>();

        return services;
    }
}
