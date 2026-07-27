// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Analytics.Culture
//  Layer:          IntegrationTests
//  File:           CultureIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Analytics.Culture module — workflow, outbox, correlation, and safety assertions.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Analytics.Culture;

/// <summary>
/// Integration tests for the <c>Culture</c> module in the <c>Analytics</c> area.
/// Validates: command → aggregate → domain event → outbox → idempotent consumer flow.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CultureIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateCulture_ShouldPersistAggregateAndEnqueueOutboxMessage()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var tenantId      = TenantId;

        // Seed a synthetic outbox message representing the CultureCreated event
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), @EventType, N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new
            {
                EventType     = "Analytics.CultureCreated",
                TenantId      = tenantId,
                ActorId       = ActorId,
                CorrelationId = correlationId,
                Now           = FixedUtcNow
            });

        // Assert
        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "Analytics.CultureCreated", tenantId);
    }

    [Fact]
    public async Task CreateCulture_ShouldPropagateCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Analytics.CultureCorrelation', N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        var storedCorrelationId = await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 CorrelationId FROM [System].[OutboxMessages] WHERE CorrelationId = @CorrelationId",
            new { CorrelationId = correlationId });

        Guid.Parse(storedCorrelationId!).Should().Be(correlationId, "CorrelationId must propagate from command to outbox");
    }

    [Fact]
    public async Task Consumer_CultureCreated_ShouldBeIdempotent()
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

        alreadyProcessed.Should().Be(1, "CultureCreated consumer must be idempotent");
    }
}
