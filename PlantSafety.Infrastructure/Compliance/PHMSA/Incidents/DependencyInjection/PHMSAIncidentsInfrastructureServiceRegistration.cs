// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Infrastructure
//  File:           PHMSAIncidentsInfrastructureServiceRegistration.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.PHMSA.Incidents
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IIncidentReportRepository interfaces (Domain write-side ->
//                  IncidentReportDapperRepository, Application read-side ->
//                  IncidentReportReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent.
//
//                  NOT YET WIRED into the central InfrastructureServiceRegistration.cs -- two
//                  parallel efforts are concurrently adding AddPHMSAClassificationInfrastructure()
//                  / AddPHMSAPackagingInfrastructure() / AddPHMSAPlacardingInfrastructure() /
//                  AddPHMSAShippingPapersInfrastructure() calls to that same shared composition
//                  root; wiring AddPHMSAIncidentsInfrastructure() in here as well was deliberately
//                  deferred to a single follow-up pass that adds all the PHMSA registration calls
//                  together, to avoid a merge collision on that shared file.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.PHMSA.Incidents.Persistence;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Incidents.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.PHMSA.Incidents module.</summary>
public static class PHMSAIncidentsInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.PHMSA.Incidents Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPHMSAIncidentsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces.IIncidentReportRepository, IncidentReportDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.PHMSA.Incidents.Repositories.IIncidentReportRepository, IncidentReportReadDapperRepository>();

        return services;
    }
}
