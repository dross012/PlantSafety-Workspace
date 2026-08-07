// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Infrastructure
//  File:           PHMSAPackagingInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.PHMSA.Packaging
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IPackagingSpecificationRepository interfaces (Domain
//                  write-side -> PackagingSpecificationDapperRepository, Application read-side ->
//                  PackagingSpecificationReadDapperRepository) using fully-qualified names to
//                  avoid an ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.PHMSA.Packaging.Persistence;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Packaging.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.PHMSA.Packaging module.</summary>
public static class PHMSAPackagingInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.PHMSA.Packaging Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPHMSAPackagingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces.IPackagingSpecificationRepository, PackagingSpecificationDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.PHMSA.Packaging.Repositories.IPackagingSpecificationRepository, PackagingSpecificationReadDapperRepository>();

        return services;
    }
}
