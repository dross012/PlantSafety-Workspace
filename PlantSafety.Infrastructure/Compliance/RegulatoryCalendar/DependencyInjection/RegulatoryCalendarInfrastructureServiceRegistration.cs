// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Infrastructure
//  File:           RegulatoryCalendarInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Compliance.RegulatoryCalendar
//                  Infrastructure-layer repository implementations. Registers both
//                  same-simple-name IRegulatoryCalendarRepository interfaces (Domain write-side ->
//                  RegulatoryCalendarAggregateDapperRepository, Application read-side ->
//                  RegulatoryCalendarReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error, exactly like
//                  PermitsInfrastructureServiceRegistration.cs's identical precedent. Wired into
//                  the central InfrastructureServiceRegistration.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Persistence;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Compliance.RegulatoryCalendar module.</summary>
public static class RegulatoryCalendarInfrastructureServiceRegistration
{
    /// <summary>Adds Compliance.RegulatoryCalendar Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddRegulatoryCalendarInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces.IRegulatoryCalendarRepository, RegulatoryCalendarAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.Compliance.RegulatoryCalendar.Repositories.IRegulatoryCalendarRepository, RegulatoryCalendarReadDapperRepository>();

        return services;
    }
}
