// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Analytics
//  Layer:          Infrastructure
//  File:           AnalyticsConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Analytics area.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Analytics.Culture.Messaging;
using PlantSafety.Infrastructure.Analytics.Dashboard.Messaging;
using PlantSafety.Infrastructure.Analytics.Engagement.Messaging;
using PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Messaging;
using PlantSafety.Infrastructure.Analytics.ScoreCards.Messaging;

namespace PlantSafety.Infrastructure.Messaging.Consumers.Analytics;

/// <summary>
/// Registers all MassTransit consumers for the <c>Analytics</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class AnalyticsConsumerRegistration
{
    /// <summary>Adds all Analytics consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<CultureCreatedConsumer>();
        cfg.AddConsumer<DashboardCreatedConsumer>();
        cfg.AddConsumer<EngagementCreatedConsumer>();
        cfg.AddConsumer<PredictiveAnalyticsCreatedConsumer>();
        cfg.AddConsumer<ScoreCardsCreatedConsumer>();
    }
}
