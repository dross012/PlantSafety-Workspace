// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Commands
//  Layer:          IntegrationTests
//  File:           CommandCorrelationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies that CorrelationId and CausationId propagate from commands through to domain events and the outbox.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.Assertions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Commands;

/// <summary>
/// Cross-cutting integration tests that assert correlation and causation lineage
/// propagation from the API command boundary through domain events to the outbox.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CommandCorrelationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Command_WithCorrelationId_ShouldPropagateToOutbox()
    {
        // This test seeds an outbox message with the known correlation ID and verifies
        // CorrelationAssertions finds it — establishing the correlation propagation contract.
        var customCorrelationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Commands.CorrelationTest', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = customCorrelationId, Now = FixedUtcNow });

        await CorrelationAssertions.AssertCorrelationPropagatedToOutboxAsync(
            ConnectionString, customCorrelationId, "Commands.CorrelationTest");
    }
}
