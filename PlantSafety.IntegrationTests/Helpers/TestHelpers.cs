// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Helpers
//  Layer:          IntegrationTests
//  File:           TestHelpers.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    General-purpose test helpers and utility methods for integration tests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Helpers;

/// <summary>
/// General-purpose helper methods shared across integration tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>Generates a new deterministic test correlation identifier.</summary>
    public static Guid NewCorrelationId() => Guid.NewGuid();

    /// <summary>
    /// Waits up to <paramref name="timeoutMs"/> milliseconds for a condition to become true.
    /// Used when asserting asynchronous background job side effects.
    /// </summary>
    public static async Task WaitForConditionAsync(
        Func<Task<bool>> condition,
        int timeoutMs       = 5000,
        int pollIntervalMs  = 100)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (await condition()) return;
            await Task.Delay(pollIntervalMs);
        }
        throw new TimeoutException($"Condition was not met within {timeoutMs}ms.");
    }
}
