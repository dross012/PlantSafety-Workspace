// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.DependencyInjection
//  Layer:          Infrastructure
//  File:           PhysicalAccessControlInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the PhysicalAccessControl
//                  Infrastructure layer (6 entity repositories, each implementing both its
//                  Domain write-repository interface and, where a read-model query is
//                  defined, its Application read-model repository interface, against the
//                  same Dapper repository class). Call
//                  <c>services.AddPhysicalAccessControlInfrastructure()</c> from the host
//                  startup alongside <c>services.AddInfrastructure(configuration)</c>.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.Repositories;
using PlantSafety.Infrastructure.PhysicalAccessControl.Persistence;
using PlantSafety.Infrastructure.PhysicalAccessControl.AccessPoints.Persistence;
using PlantSafety.Infrastructure.PhysicalAccessControl.Credentials.Persistence;
using PlantSafety.Infrastructure.PhysicalAccessControl.AccessEvents.Persistence;
using PlantSafety.Infrastructure.PhysicalAccessControl.SafetyZones.Persistence;
using PlantSafety.Infrastructure.PhysicalAccessControl.AccessAnomalies.Persistence;
using PlantSafety.Infrastructure.PhysicalAccessControl.RestrictedZonePolicies.Persistence;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.DependencyInjection;

/// <summary>Per-module DI entry point for the PhysicalAccessControl Infrastructure layer.</summary>
public static class PhysicalAccessControlInfrastructureServiceRegistration
{
    /// <summary>Registers all PhysicalAccessControl Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddPhysicalAccessControlInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces.IAccessPointRepository, AccessPointDapperRepository>();
        services.AddScoped<PlantSafety.Application.PhysicalAccessControl.AccessPoints.Repositories.IAccessPointReadRepository, AccessPointDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces.ICredentialRepository, CredentialDapperRepository>();
        services.AddScoped<PlantSafety.Application.PhysicalAccessControl.Credentials.Repositories.ICredentialReadRepository, CredentialDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces.IAccessEventRepository, AccessEventDapperRepository>();
        services.AddScoped<PlantSafety.Application.PhysicalAccessControl.AccessEvents.Repositories.IAccessEventReadRepository, AccessEventDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces.ISafetyZoneRepository, SafetyZoneDapperRepository>();
        services.AddScoped<PlantSafety.Application.PhysicalAccessControl.SafetyZones.Repositories.ISafetyZoneReadRepository, SafetyZoneDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces.IAccessAnomalyRepository, AccessAnomalyDapperRepository>();
        services.AddScoped<PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Repositories.IAccessAnomalyReadRepository, AccessAnomalyDapperRepository>();
        services.AddScoped<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces.IRestrictedZonePolicyRepository, RestrictedZonePolicyDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<IPhysicalAccessControlRepository, PhysicalAccessControlRepository>();

        return services;
    }
}
