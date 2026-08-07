// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Infrastructure
//  File:           PHMSAPlacardingInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.PHMSA.Placarding
//                  Infrastructure-layer repository implementations plus the stateless
//                  PlacardingEngine domain service. Registers both same-simple-name
//                  IHazmatContainerRepository interfaces (Domain write-side ->
//                  HazmatContainerDapperRepository, Application read-side ->
//                  HazmatContainerReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, mirroring
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent.
//
//                  PENDING WIRING: this extension is NOT YET called from the central
//                  InfrastructureServiceRegistration.cs composition root -- a parallel effort is
//                  simultaneously adding AddPHMSAClassificationInfrastructure() /
//                  AddPHMSAPackagingInfrastructure() to that same shared file, so wiring
//                  AddPHMSAPlacardingInfrastructure() / AddPHMSAShippingPapersInfrastructure() in
//                  here was deliberately deferred to a follow-up pass that adds all four calls
//                  together in one edit, avoiding a merge collision on the shared file.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Services;
using PlantSafety.Infrastructure.Compliance.PHMSA.Placarding.Persistence;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Placarding.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.PHMSA.Placarding module.</summary>
public static class PHMSAPlacardingInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.PHMSA.Placarding Dapper repositories and domain services to the service collection.</summary>
    public static IServiceCollection AddPHMSAPlacardingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces.IHazmatContainerRepository, HazmatContainerDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.PHMSA.Placarding.Repositories.IHazmatContainerRepository, HazmatContainerReadDapperRepository>();

        services.AddSingleton<PlacardingEngine>();

        return services;
    }
}
