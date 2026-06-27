// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           CorrelationAssertions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Assertion helpers that verify CorrelationId and CausationId lineage propagation.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.Assertions;

/// <summary>
/// Assertion helpers for verifying that <c>CorrelationId</c> and <c>CausationId</c>
/// propagate correctly through the entire command → domain event → outbox pipeline.
/// </summary>
public static class CorrelationAssertions
{
    /// <summary>
    /// Asserts that at least one outbox message with the given <paramref name="correlationId"/>
    /// and <paramref name="eventType"/> exists in the database.
    /// </summary>
    public static async Task AssertCorrelationPropagatedToOutboxAsync(
        string connectionString,
        Guid correlationId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM [System].[OutboxMessages]
              WHERE CorrelationId = @CorrelationId AND EventType = @EventType",
            new { CorrelationId = correlationId, EventType = eventType });

        count.Should().BeGreaterThan(0,
            $"CorrelationId {correlationId} should propagate to outbox for event '{eventType}'");
    }
}
