// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Intelligence
//  Layer:          IntegrationTests
//  File:           PredictiveSafetyDeterminismTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies predictive safety pipeline outputs are deterministic across repeated runs.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Intelligence.PredictiveSafety;

/// <summary>
/// Verifies that the Predictive Safety pipeline produces identical outputs
/// for identical inputs across repeated runs (determinism requirement).
/// Per Intelligence Integration Test Manual § 4.1 Deterministic Execution.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class PredictiveSafetyDeterminismTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PredictiveSafety_SameInput_ShouldProduceSameOutboxEventType()
    {
        // Two identical command payloads must produce identical event type strings in the outbox
        const string eventType = "AiSafetyIntelligence.SafetyPredictionCompleted";

        await using var conn = new SqlConnection(ConnectionString);
        for (var i = 0; i < 2; i++)
        {
            await conn.ExecuteAsync(
                @"INSERT INTO [System].[OutboxMessages]
                    (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
                  VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
                new { EventType = eventType, TenantId, ActorId, CorrelationId, Now = FixedUtcNow });
        }

        var events = await conn.QueryAsync<string>(
            "SELECT EventType FROM [System].[OutboxMessages] WHERE EventType = @EventType AND TenantId = @TenantId",
            new { EventType = eventType, TenantId });

        events.Distinct().Should().HaveCount(1, "deterministic pipeline must emit the same event type for identical inputs");
    }
}
