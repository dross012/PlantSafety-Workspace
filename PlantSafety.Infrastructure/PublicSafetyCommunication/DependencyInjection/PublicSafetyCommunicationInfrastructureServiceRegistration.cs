// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication
//  Layer:          Infrastructure
//  File:           PublicSafetyCommunicationInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the PublicSafetyCommunication
//                  Infrastructure-layer repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish (mirrors
//                  PermitToWorkInfrastructureServiceRegistration.cs).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.PublicSafetyCommunication.AgencyDirectory.Persistence;
using PlantSafety.Infrastructure.PublicSafetyCommunication.IncidentNotificationDispatch.Persistence;
using PlantSafety.Infrastructure.PublicSafetyCommunication.PublicAlertBroadcast.Persistence;
using PlantSafety.Infrastructure.PublicSafetyCommunication.CommunicationLog.Persistence;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the PublicSafetyCommunication module.</summary>
public static class PublicSafetyCommunicationInfrastructureServiceRegistration
{
    /// <summary>Adds PublicSafetyCommunication Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddPublicSafetyCommunicationInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces.IPublicSafetyAgencyRepository, PublicSafetyAgencyDapperRepository>();
        services.AddScoped<PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Repositories.IPublicSafetyAgencyRepository, PublicSafetyAgencyReadDapperRepository>();

        services.AddScoped<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces.IIncidentNotificationRepository, IncidentNotificationDapperRepository>();
        services.AddScoped<PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Repositories.IIncidentNotificationRepository, IncidentNotificationReadDapperRepository>();

        services.AddScoped<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces.IPublicAlertBroadcastRepository, PublicAlertBroadcastDapperRepository>();
        services.AddScoped<PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Repositories.IPublicAlertBroadcastRepository, PublicAlertBroadcastReadDapperRepository>();

        services.AddScoped<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces.ICommunicationLogEntryRepository, CommunicationLogEntryDapperRepository>();
        services.AddScoped<PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Repositories.ICommunicationLogEntryRepository, CommunicationLogEntryReadDapperRepository>();

        return services;
    }
}
