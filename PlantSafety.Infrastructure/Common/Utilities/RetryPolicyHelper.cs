// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           RetryPolicyHelper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Exponential back-off retry helper for transient infrastructure failures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Utilities;

/// <summary>
/// Exponential back-off retry helper for transient infrastructure failures
/// (SQL timeouts, transient messaging errors). Uses bounded retry count.
/// </summary>
public static class RetryPolicyHelper
{
    /// <summary>Executes <paramref name="action"/> with bounded exponential back-off retries.</summary>
    public static async Task ExecuteWithRetryAsync(
        Func<Task> action,
        int maxRetries = InfrastructureConstants.DefaultRetryCount,
        int baseDelayMs = InfrastructureConstants.RetryBaseDelayMs,
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    /// <summary>Executes <paramref name="func"/> with bounded exponential back-off retries and returns a result.</summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> func,
        int maxRetries = InfrastructureConstants.DefaultRetryCount,
        int baseDelayMs = InfrastructureConstants.RetryBaseDelayMs,
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await func();
            }
            catch (Exception) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
            }
        }

        return await func();
    }
}
