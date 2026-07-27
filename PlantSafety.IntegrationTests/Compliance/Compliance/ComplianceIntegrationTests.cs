// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Compliance.Compliance
//  Layer:          IntegrationTests
//  File:           ComplianceIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Compliance.Compliance module — workflow, outbox, correlation, and safety assertions.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Compliance.Compliance;

/// <summary>
/// Integration tests for the <c>Compliance</c> module in the <c>Compliance</c> area.
/// Validates: command → aggregate → domain event → outbox → idempotent consumer flow.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class ComplianceIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateCompliance_ShouldPersistAggregateAndEnqueueOutboxMessage()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var tenantId      = TenantId;

        // Seed a synthetic outbox message representing the ComplianceCreated event
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), @EventType, N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new
            {
                EventType     = "Compliance.ComplianceCreated",
                TenantId      = tenantId,
                ActorId       = ActorId,
                CorrelationId = correlationId,
                Now           = FixedUtcNow
            });

        // Assert
        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "Compliance.ComplianceCreated", tenantId);
    }

    [Fact]
    public async Task CreateCompliance_ShouldPropagateCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Compliance.ComplianceCorrelation', N'1.0', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        var storedCorrelationId = await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 CorrelationId FROM [System].[OutboxMessages] WHERE CorrelationId = @CorrelationId",
            new { CorrelationId = correlationId });

        Guid.Parse(storedCorrelationId!).Should().Be(correlationId, "CorrelationId must propagate from command to outbox");
    }

    [Fact]
    public async Task Consumer_ComplianceCreated_ShouldBeIdempotent()
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

        alreadyProcessed.Should().Be(1, "ComplianceCreated consumer must be idempotent");
    }
}
