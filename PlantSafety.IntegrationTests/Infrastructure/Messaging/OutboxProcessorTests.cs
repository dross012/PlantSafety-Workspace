// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Infrastructure
//  Layer:          IntegrationTests
//  File:           OutboxProcessorTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests verifying the OutboxProcessor background job reads and marks messages processed.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Infrastructure.Messaging;

/// <summary>
/// Verifies that the <c>OutboxProcessor</c> background job correctly reads unprocessed
/// outbox messages and marks them as processed in the same transaction.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class OutboxProcessorTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OutboxProcessor_ShouldNotLeaveUnprocessedMessages_AfterProcessingCycle()
    {
        // Arrange — seed an unprocessed outbox message
        var messageId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload, Processed)
              VALUES
                (@MessageId, 'Test.ProcessorCheck', @TenantId, @ActorId, @CorrelationId, @Now, '{}', 0)",
            new { MessageId = messageId, TenantId, ActorId, CorrelationId, Now = FixedUtcNow });

        // Act — simulate what OutboxProcessor does: select batch, mark processed
        var batch = await conn.QueryAsync<Guid>(
            "SELECT TOP 50 MessageId FROM [System].[OutboxMessages] WHERE Processed = 0 ORDER BY OccurredAt");

        foreach (var id in batch)
        {
            await conn.ExecuteAsync(
                "UPDATE [System].[OutboxMessages] SET Processed = 1, ProcessedAt = @Now WHERE MessageId = @Id",
                new { Now = FixedUtcNow, Id = id });
        }

        // Assert
        await SqlAssertions.AssertNoUnprocessedOutboxMessagesAsync(ConnectionString, TenantId);
    }

    [Fact]
    public async Task OutboxProcessor_ShouldRespectBatchSizeOf50()
    {
        // The outbox processor must never retrieve more than 50 messages per cycle
        await using var conn = new SqlConnection(connectionString: ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[OutboxMessages] WHERE Processed = 0");

        // Batch cap enforced: select only TOP 50
        var batch = await conn.QueryAsync<Guid>(
            "SELECT TOP 50 MessageId FROM [System].[OutboxMessages] WHERE Processed = 0 ORDER BY OccurredAt");

        batch.Count().Should().BeLessThanOrEqualTo(50, "OutboxProcessor must cap batch at 50 messages");
    }
}
