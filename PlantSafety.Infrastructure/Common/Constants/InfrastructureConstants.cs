// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           InfrastructureConstants.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Shared constant values for the Infrastructure layer.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Constants;

/// <summary>Shared constants for the Infrastructure layer.</summary>
public static class InfrastructureConstants
{
    /// <summary>Schema version applied to all canonical event envelopes.</summary>
    public const string EventSchemaVersion = "2026.1";

    /// <summary>Default retry count for transient failures.</summary>
    public const int DefaultRetryCount = 3;

    /// <summary>Base delay in milliseconds for exponential back-off retry.</summary>
    public const int RetryBaseDelayMs = 200;

    /// <summary>Outbox processing batch size.</summary>
    public const int OutboxBatchSize = 50;

    /// <summary>Outbox processor interval in seconds.</summary>
    public const int OutboxProcessorIntervalSeconds = 5;

    /// <summary>Dead-letter topic suffix.</summary>
    public const string DeadLetterSuffix = "/$DeadLetterQueue";

    /// <summary>
    /// Default RabbitMQ prefetch count applied bus-wide (see InfrastructureServiceRegistration's
    /// <c>x.UsingRabbitMq</c> configuration). Bounds how many unacknowledged messages MassTransit
    /// pulls per receive endpoint, which is this codebase's single chosen mechanism for capping
    /// consumer parallelism (Handbook §7 Performance &amp; Stress -- Phase 5 §7.2 "parallel
    /// consumers") -- applied once, centrally, rather than a per-consumer ConcurrentMessageLimit
    /// scattered across the 17 *ConsumerRegistration.Register(cfg) files, since no per-consumer
    /// concurrency convention existed previously.
    /// </summary>
    public const ushort DefaultRabbitMqPrefetchCount = 16;
}
