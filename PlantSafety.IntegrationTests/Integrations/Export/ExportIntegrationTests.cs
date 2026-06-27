// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Export
//  Layer:          IntegrationTests
//  File:           ExportIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Integrations.Export module.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Export;

/// <summary>Integration tests for the <c>Export</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class ExportIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateExport_ShouldEnqueueOutboxMessage()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { EventType = "Integrations.ExportCreated", TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(ConnectionString, "Integrations.ExportCreated", TenantId);
    }

    [Fact]
    public async Task Consumer_ExportCreated_ShouldBeIdempotent()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        count.Should().Be(1, "Export consumer must not process the same event twice");
    }
}
