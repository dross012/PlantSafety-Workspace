// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           EventBusOptions.cs
//  Created:        2026-07-27
//  Description:    Binds to the "EventBus" configuration section. Backs the SQL-queue
//                  transport (dbo.EventQueue / ProcessedEvents / EventRetryState /
//                  EventDeadLetterQueue) and the retry/backoff/poison-message policy.
// ============================================================================
namespace Shared.EventBus.Infrastructure.Options;

/// <summary>Configuration for the SQL-backed Shared.EventBus transport.</summary>
public sealed class EventBusOptions
{
    /// <summary>Configuration section name (matches the "Shared:X" convention used by Shared.AuditLogging/Shared.SafetyData).</summary>
    public const string SectionName = "Shared:EventBus";

    /// <summary>Connection string to the database hosting dbo.EventQueue and related tables.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Maximum delivery attempts (including the first) before an event is moved to the dead-letter queue.</summary>
    public int MaxDeliveryAttempts { get; set; } = 5;

    /// <summary>Base delay for exponential backoff between retry attempts.</summary>
    public TimeSpan BackoffBaseDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Upper bound applied to the computed exponential backoff delay.</summary>
    public TimeSpan BackoffMaxDelay { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Number of consecutive attempts that all fail with the same exception type/message
    /// before the message is flagged <c>IsPoisonMessage = true</c> when dead-lettered
    /// (in addition to the ordinary max-attempts dead-letter path).
    /// </summary>
    public int PoisonMessageRepeatThreshold { get; set; } = 3;

    /// <summary>Command timeout, in seconds, applied to queue/dispatch stored-procedure calls.</summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>Computes the exponential backoff delay for the given (1-based) attempt number, bounded by <see cref="BackoffMaxDelay"/>.</summary>
    public TimeSpan ComputeBackoff(int attemptNumber)
    {
        if (attemptNumber < 1) attemptNumber = 1;
        var factor = Math.Pow(2, attemptNumber - 1);
        var delayMs = Math.Min(BackoffBaseDelay.TotalMilliseconds * factor, BackoffMaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(delayMs);
    }
}
