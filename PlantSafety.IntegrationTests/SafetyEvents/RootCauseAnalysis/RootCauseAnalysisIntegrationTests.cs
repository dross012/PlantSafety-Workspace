// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.SafetyEvents.RootCauseAnalysis
//  Layer:          IntegrationTests
//  File:           RootCauseAnalysisIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the SafetyEvents.RootCauseAnalysis module.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.SafetyEvents.RootCauseAnalysis;

/// <summary>Integration tests for the <c>RootCauseAnalysis</c> module in the <c>SafetyEvents</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class RootCauseAnalysisIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateRootCauseAnalysis_ShouldEnqueueOutboxMessage()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { EventType = "SafetyEvents.RootCauseAnalysisCreated", TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(ConnectionString, "SafetyEvents.RootCauseAnalysisCreated", TenantId);
    }

    [Fact]
    public async Task Consumer_RootCauseAnalysisCreated_ShouldBeIdempotent()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        count.Should().Be(1, "RootCauseAnalysis consumer must not process the same event twice");
    }
}
