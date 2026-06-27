// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           SqlAssertions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    SQL assertion helpers that query the Testcontainers database to verify persistence.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// SQL assertion helpers for verifying database state after command execution.
/// All queries are parameterized — no string-built SQL.
/// </summary>
public static class SqlAssertions
{
    /// <summary>
    /// Asserts at least one outbox message exists for the given event type and tenant.
    /// </summary>
    public static async Task AssertOutboxContainsAsync(
        string connectionString,
        string eventType,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[OutboxMessages] WHERE EventType = @EventType AND TenantId = @TenantId",
            new { EventType = eventType, TenantId = tenantId });

        count.Should().BeGreaterThan(0,
            $"outbox should contain at least one '{eventType}' event for tenant {tenantId}");
    }

    /// <summary>Asserts the specified event is recorded in the ProcessedEvents idempotency table.</summary>
    public static async Task AssertEventProcessedAsync(
        string connectionString,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        count.Should().Be(1, $"event {eventId} should be marked as processed exactly once");
    }

    /// <summary>Asserts no unprocessed outbox messages remain for the tenant.</summary>
    public static async Task AssertNoUnprocessedOutboxMessagesAsync(
        string connectionString,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[OutboxMessages] WHERE TenantId = @TenantId AND Processed = 0",
            new { TenantId = tenantId });

        count.Should().Be(0, "all outbox messages for the tenant should be processed");
    }
}
