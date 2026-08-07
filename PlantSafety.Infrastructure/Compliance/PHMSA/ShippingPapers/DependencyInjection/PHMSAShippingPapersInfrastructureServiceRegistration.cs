// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Infrastructure
//  File:           PHMSAShippingPapersInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.PHMSA.ShippingPapers
//                  Infrastructure-layer repository implementations plus the stateless
//                  ShippingPaperGenerator domain service. Also registers the Segregation module's
//                  stateless SegregationEngine domain service here -- Segregation has no
//                  persisted aggregate and therefore no Infrastructure layer of its own (per the
//                  PHMSA sub-area build-out spec), so its sole DI need (the engine instance
//                  consumed by PlantSafety.Application.Compliance.PHMSA.Segregation.Handlers) is
//                  satisfied by this registration instead of a dedicated one. Registers both
//                  same-simple-name IHazmatShipmentRepository interfaces (Domain write-side ->
//                  HazmatShipmentDapperRepository, Application read-side ->
//                  HazmatShipmentReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, mirroring
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent.
//
//                  PENDING WIRING: this extension is NOT YET called from the central
//                  InfrastructureServiceRegistration.cs composition root -- see the matching note
//                  on PHMSAPlacardingInfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Services;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Services;
using PlantSafety.Infrastructure.Compliance.PHMSA.ShippingPapers.Persistence;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.ShippingPapers.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.PHMSA.ShippingPapers (and Segregation) modules.</summary>
public static class PHMSAShippingPapersInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.PHMSA.ShippingPapers Dapper repositories and domain services to the service collection.</summary>
    public static IServiceCollection AddPHMSAShippingPapersInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces.IHazmatShipmentRepository, HazmatShipmentDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Repositories.IHazmatShipmentRepository, HazmatShipmentReadDapperRepository>();

        services.AddSingleton<ShippingPaperGenerator>();
        services.AddSingleton<SegregationEngine>();

        return services;
    }
}
