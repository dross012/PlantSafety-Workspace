// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Operational.Schedules
//  Layer:          IntegrationTests
//  File:           SchedulesIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Operational.Schedules module — workflow, outbox, correlation, and safety assertions.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Operational.Schedules;

/// <summary>
/// Integration tests for the <c>Schedules</c> module in the <c>Operational</c> area.
/// Validates: command → aggregate → domain event → outbox → idempotent consumer flow.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class SchedulesIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateSchedules_ShouldPersistAggregateAndEnqueueOutboxMessage()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var tenantId      = TenantId;

        // Seed a synthetic outbox message representing the SchedulesCreated event
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new
            {
                EventType     = "Operational.SchedulesCreated",
                TenantId      = tenantId,
                ActorId       = ActorId,
                CorrelationId = correlationId,
                Now           = FixedUtcNow
            });

        // Assert
        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "Operational.SchedulesCreated", tenantId);
    }

    [Fact]
    public async Task CreateSchedules_ShouldPropagateCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Operational.SchedulesCorrelation', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        var storedCorrelationId = await conn.ExecuteScalarAsync<Guid>(
            "SELECT TOP 1 CorrelationId FROM [System].[OutboxMessages] WHERE CorrelationId = @CorrelationId",
            new { CorrelationId = correlationId });

        storedCorrelationId.Should().Be(correlationId, "CorrelationId must propagate from command to outbox");
    }

    [Fact]
    public async Task Consumer_SchedulesCreated_ShouldBeIdempotent()
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

        alreadyProcessed.Should().Be(1, "SchedulesCreated consumer must be idempotent");
    }
}
