// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.DependencyInjection
//  Layer:          Infrastructure
//  File:           CommunitySafetyInfrastructureServiceRegistration.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the CommunitySafety Infrastructure
//                  layer (5 sub-component aggregate repositories, each implementing both its
//                  Domain write-repository interface and its Application read-model repository
//                  interface against the same Dapper repository class). Call
//                  <c>services.AddCommunitySafetyInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.Repositories;
using PlantSafety.Infrastructure.CommunitySafety.Persistence;
using PlantSafety.Infrastructure.CommunitySafety.CommunityContact.Persistence;
using PlantSafety.Infrastructure.CommunitySafety.CommunityConcern.Persistence;
using PlantSafety.Infrastructure.CommunitySafety.CommunityImpactAssessment.Persistence;
using PlantSafety.Infrastructure.CommunitySafety.CommunityNotification.Persistence;
using PlantSafety.Infrastructure.CommunitySafety.CommunityMeetingRecord.Persistence;

namespace PlantSafety.Infrastructure.CommunitySafety.DependencyInjection;

/// <summary>Per-module DI entry point for the CommunitySafety Infrastructure layer.</summary>
public static class CommunitySafetyInfrastructureServiceRegistration
{
    /// <summary>Registers all CommunitySafety Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddCommunitySafetyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.CommunitySafety.CommunityContact.Interfaces.ICommunityContactRepository, CommunityContactDapperRepository>();
        services.AddScoped<PlantSafety.Application.CommunitySafety.CommunityContact.Repositories.ICommunityContactRepository, CommunityContactDapperRepository>();
        services.AddScoped<PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces.ICommunityConcernRepository, CommunityConcernDapperRepository>();
        services.AddScoped<PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories.ICommunityConcernRepository, CommunityConcernDapperRepository>();
        services.AddScoped<PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Interfaces.ICommunityImpactAssessmentRepository, CommunityImpactAssessmentDapperRepository>();
        services.AddScoped<PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Repositories.ICommunityImpactAssessmentRepository, CommunityImpactAssessmentDapperRepository>();
        services.AddScoped<PlantSafety.Domain.CommunitySafety.CommunityNotification.Interfaces.ICommunityNotificationRepository, CommunityNotificationDapperRepository>();
        services.AddScoped<PlantSafety.Application.CommunitySafety.CommunityNotification.Repositories.ICommunityNotificationRepository, CommunityNotificationDapperRepository>();
        services.AddScoped<PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Interfaces.ICommunityMeetingRecordRepository, CommunityMeetingRecordDapperRepository>();
        services.AddScoped<PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Repositories.ICommunityMeetingRecordRepository, CommunityMeetingRecordDapperRepository>();

        // Top-level placeholder marker (no members)
        services.AddScoped<ICommunitySafetyRepository, CommunitySafetyRepository>();

        return services;
    }
}
