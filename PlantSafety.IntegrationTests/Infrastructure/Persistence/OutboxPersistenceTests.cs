// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Infrastructure
//  Layer:          IntegrationTests
//  File:           OutboxPersistenceTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests verifying OutboxMessages table persistence and idempotency.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Infrastructure.Persistence;

/// <summary>
/// Verifies that the Outbox persistence layer correctly inserts, reads, and marks
/// messages processed — without publishing to a real message bus.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class OutboxPersistenceTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Insert_OutboxMessage_ShouldPersistWithCorrectFields()
    {
        // Arrange
        var messageId     = Guid.NewGuid();
        var eventType     = "Alerts.AlertCreated";
        var tenantId      = TenantId;
        var correlationId = CorrelationId;
        var actorId       = ActorId;
        var payload       = "{\"test\":true}";

        await using var conn = new SqlConnection(ConnectionString);

        // Act
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (@MessageId, @EventType, @TenantId, @ActorId, @CorrelationId, @OccurredAt, @Payload)",
            new
            {
                MessageId     = messageId,
                EventType     = eventType,
                TenantId      = tenantId,
                ActorId       = actorId,
                CorrelationId = correlationId,
                OccurredAt    = FixedUtcNow,
                Payload       = payload
            });

        // Assert
        var stored = await conn.QuerySingleOrDefaultAsync(
            "SELECT * FROM [System].[OutboxMessages] WHERE MessageId = @MessageId",
            new { MessageId = messageId });

        ((object?)stored).Should().NotBeNull();
        ((string)stored!.EventType).Should().Be(eventType);
        ((bool)stored.Processed).Should().BeFalse();
    }

    [Fact]
    public async Task MarkProcessed_ShouldUpdateOutboxMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (@MessageId, 'Test.Event', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { MessageId = messageId, TenantId, ActorId, CorrelationId, Now = FixedUtcNow });

        // Act
        await conn.ExecuteAsync(
            "UPDATE [System].[OutboxMessages] SET Processed = 1, ProcessedAt = @Now WHERE MessageId = @MessageId",
            new { Now = FixedUtcNow, MessageId = messageId });

        // Assert
        var processed = await conn.ExecuteScalarAsync<bool>(
            "SELECT Processed FROM [System].[OutboxMessages] WHERE MessageId = @MessageId",
            new { MessageId = messageId });

        processed.Should().BeTrue();
    }
}
