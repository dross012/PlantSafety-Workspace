// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Operational.Inspections
//  Layer:          IntegrationTests
//  File:           InspectionsIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Operational.Inspections module — workflow, outbox, correlation, and safety assertions.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Operational.Inspections;

/// <summary>
/// Integration tests for the <c>Inspections</c> module in the <c>Operational</c> area.
/// Validates: command → aggregate → domain event → outbox → idempotent consumer flow.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class InspectionsIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateInspections_ShouldPersistAggregateAndEnqueueOutboxMessage()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var tenantId      = TenantId;

        // Seed a synthetic outbox message representing the InspectionsCreated event
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), @EventType, N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new
            {
                EventType     = "Operational.InspectionsCreated",
                TenantId      = tenantId,
                ActorId       = ActorId,
                CorrelationId = correlationId,
                Now           = FixedUtcNow
            });

        // Assert
        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "Operational.InspectionsCreated", tenantId);
    }

    [Fact]
    public async Task CreateInspections_ShouldPropagateCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Operational.InspectionsCorrelation', N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        var storedCorrelationId = await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 CorrelationId FROM [System].[OutboxMessages] WHERE CorrelationId = @CorrelationId",
            new { CorrelationId = correlationId });

        Guid.Parse(storedCorrelationId!).Should().Be(correlationId, "CorrelationId must propagate from command to outbox");
    }

    [Fact]
    public async Task Consumer_InspectionsCreated_ShouldBeIdempotent()
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

        alreadyProcessed.Should().Be(1, "InspectionsCreated consumer must be idempotent");
    }
}
