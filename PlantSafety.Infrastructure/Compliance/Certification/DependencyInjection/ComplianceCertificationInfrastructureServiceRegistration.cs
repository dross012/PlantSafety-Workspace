// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Infrastructure
//  File:           ComplianceCertificationInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.Certification
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name ICertificationRepository interfaces (Domain write-side ->
//                  CertificationDapperRepository, Application read-side ->
//                  CertificationReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Certification.Persistence;

namespace PlantSafety.Infrastructure.Compliance.Certification.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.Certification module.</summary>
public static class ComplianceCertificationInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.Certification Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddComplianceCertificationInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.Certification.Interfaces.ICertificationRepository, CertificationDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.Certification.Repositories.ICertificationRepository, CertificationReadDapperRepository>();

        return services;
    }
}
