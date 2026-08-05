// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.SafetyEvents.LocalEmergencyCoordination
//  Layer:          IntegrationTests
//  File:           LocalEmergencyCoordinationIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the SafetyEvents.LocalEmergencyCoordination module,
//                  following the same outbox-enqueue / consumer-idempotency pattern already
//                  established for every sibling SafetyEvents sub-aggregate (EmergencyManagement,
//                  CrisisManagement, DrillsAndExercises, NearMisses, Observation, RootCauseAnalysis)
//                  -- see EmergencyManagementIntegrationTests.cs. Deterministic, parallel-safe,
//                  additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.SafetyEvents.LocalEmergencyCoordination;

/// <summary>Integration tests for the <c>LocalEmergencyCoordination</c> module in the <c>SafetyEvents</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class LocalEmergencyCoordinationIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OpenLocalEmergencyCoordinationCase_ShouldEnqueueOutboxMessage()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { EventType = "SafetyEvents.LocalEmergencyCoordinationCreated", TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(ConnectionString, "SafetyEvents.LocalEmergencyCoordinationCreated", TenantId);
    }

    [Fact]
    public async Task MutualAidAgreementEstablished_ShouldEnqueueOutboxMessage()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { EventType = "SafetyEvents.LocalEmergencyCoordination.MutualAidAgreementEstablished", TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(ConnectionString, "SafetyEvents.LocalEmergencyCoordination.MutualAidAgreementEstablished", TenantId);
    }

    [Fact]
    public async Task Consumer_LocalEmergencyCoordinationCreated_ShouldBeIdempotent()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        count.Should().Be(1, "LocalEmergencyCoordination's LocalEmergencyCoordinationCreatedConsumer must not process the same event twice");
    }

    [Fact]
    public async Task Consumer_LocalEmergencyCoordinationCreated_SecondInsertOfSameEventId_ShouldViolatePrimaryKey()
    {
        // Reinforces idempotency at the storage layer: [System].[ProcessedEvents].EventId is the
        // PRIMARY KEY, so a duplicate-delivery re-insert attempt (the scenario
        // CrossModuleIdempotentConsumerBase / LocalEmergencyCoordinationCreatedConsumer guards
        // against) must fail fast rather than silently duplicate-processing the event.
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var act = async () => await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow.AddSeconds(1) });

        await act.Should().ThrowAsync<SqlException>("EventId is the PRIMARY KEY of [System].[ProcessedEvents]");
    }
}
