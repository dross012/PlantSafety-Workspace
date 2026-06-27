// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           MessagingOptions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for Azure Service Bus messaging.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Strongly typed Azure Service Bus messaging options.</summary>
public sealed class MessagingOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Messaging";

    /// <summary>Azure Service Bus namespace connection string.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Name of the primary topic used for integration events.</summary>
    public string TopicName { get; init; } = "plantsafety-events";

    /// <summary>Subscription name for this service instance.</summary>
    public string SubscriptionName { get; init; } = string.Empty;

    /// <summary>Maximum concurrent messages to process.</summary>
    public int MaxConcurrentCalls { get; init; } = 16;

    /// <summary>Maximum delivery count before dead-lettering.</summary>
    public int MaxDeliveryCount { get; init; } = 10;

    /// <summary>Lock duration for message lease in seconds.</summary>
    public int LockDurationSeconds { get; init; } = 300;
}
