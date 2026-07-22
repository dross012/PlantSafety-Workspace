// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.DependencyInjection
//  Layer:          Infrastructure
//  File:           SafetyCultureInfrastructureServiceRegistration.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Per-module DI registration entry point for the SafetyCulture
//                  Infrastructure layer (2 Domain aggregate repositories + 8 Application
//                  read repositories). Deliberately separate from the pre-existing
//                  Analytics.Culture module's registrations (see
//                  PlantSafety.Infrastructure\Analytics\Culture). Call
//                  <c>services.AddSafetyCultureInfrastructure()</c> from the host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by
//                  the shared InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  that file is out of scope for this pass; wiring the call site there is a
//                  required, tracked follow-up. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Repositories;
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Repositories;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.Interfaces;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.NearMissReportingRate.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.PpeComplianceRate.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.StopWorkActions.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.TrainingCompletionRate.Persistence;
using PlantSafety.Infrastructure.SafetyCulture.StopWorkEvent.Persistence;

namespace PlantSafety.Infrastructure.SafetyCulture.DependencyInjection;

/// <summary>Per-module DI entry point for the SafetyCulture Infrastructure layer.</summary>
public static class SafetyCultureInfrastructureServiceRegistration
{
    /// <summary>Registers all SafetyCulture Infrastructure repositories into the DI container.</summary>
    public static IServiceCollection AddSafetyCultureInfrastructure(this IServiceCollection services)
    {
        // Domain aggregate repositories
        services.AddScoped<ICultureAssessmentRepository, CultureAssessmentDapperRepository>();
        services.AddScoped<IStopWorkEventRepository, StopWorkEventDapperRepository>();

        // Application metric read repositories
        services.AddScoped<IBehavioralObservationCompletionRepository, BehavioralObservationCompletionDapperRepository>();
        services.AddScoped<ILeadershipEngagementFrequencyRepository, LeadershipEngagementFrequencyDapperRepository>();
        services.AddScoped<INearMissReportingRateRepository, NearMissReportingRateDapperRepository>();
        services.AddScoped<IPpeComplianceRateRepository, PpeComplianceRateDapperRepository>();
        services.AddScoped<ISafetyMeetingParticipationRepository, SafetyMeetingParticipationDapperRepository>();
        services.AddScoped<ISafetySuggestionSubmissionsRepository, SafetySuggestionSubmissionsDapperRepository>();
        services.AddScoped<IStopWorkActionsRepository, StopWorkActionsDapperRepository>();
        services.AddScoped<ITrainingCompletionRateRepository, TrainingCompletionRateDapperRepository>();

        return services;
    }
}
