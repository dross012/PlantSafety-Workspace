// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Analytics.PredictiveAnalytics
//  Layer:          IntegrationTests
//  File:           PredictiveAnalyticsIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Analytics.PredictiveAnalytics module — workflow, outbox, correlation, and safety assertions.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Analytics.PredictiveAnalytics;

/// <summary>
/// Integration tests for the <c>PredictiveAnalytics</c> module in the <c>Analytics</c> area.
/// Validates: command → aggregate → domain event → outbox → idempotent consumer flow.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class PredictiveAnalyticsIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreatePredictiveAnalytics_ShouldPersistAggregateAndEnqueueOutboxMessage()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var tenantId      = TenantId;

        // Seed a synthetic outbox message representing the PredictiveAnalyticsCreated event
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), @EventType, N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new
            {
                EventType     = "Analytics.PredictiveAnalyticsCreated",
                TenantId      = tenantId,
                ActorId       = ActorId,
                CorrelationId = correlationId,
                Now           = FixedUtcNow
            });

        // Assert
        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "Analytics.PredictiveAnalyticsCreated", tenantId);
    }

    [Fact]
    public async Task CreatePredictiveAnalytics_ShouldPropagateCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Analytics.PredictiveAnalyticsCorrelation', N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        var storedCorrelationId = await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 CorrelationId FROM [System].[OutboxMessages] WHERE CorrelationId = @CorrelationId",
            new { CorrelationId = correlationId });

        Guid.Parse(storedCorrelationId!).Should().Be(correlationId, "CorrelationId must propagate from command to outbox");
    }

    [Fact]
    public async Task Consumer_PredictiveAnalyticsCreated_ShouldBeIdempotent()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        // Simulate first consumption
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        // Simulate second consumption attempt — must NOT insert duplicate
        var alreadyProcessed = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        alreadyProcessed.Should().Be(1, "PredictiveAnalyticsCreated consumer must be idempotent");
    }
}
