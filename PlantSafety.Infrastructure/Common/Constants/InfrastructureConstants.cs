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
}
