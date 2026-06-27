// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Infrastructure
//  Layer:          IntegrationTests
//  File:           ProcessedEventsPersistenceTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Tests for ProcessedEvents idempotency table — insert and duplicate detection.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Infrastructure.Persistence;

/// <summary>
/// Verifies that the <c>System.ProcessedEvents</c> idempotency table correctly
/// records processed event identifiers and rejects duplicates.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class ProcessedEventsPersistenceTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Insert_ProcessedEvent_ShouldPersist()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        await SqlAssertions.AssertEventProcessedAsync(ConnectionString, eventId);
    }

    [Fact]
    public async Task Insert_DuplicateProcessedEvent_ShouldViolateUniqueConstraint()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var act = async () => await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        await act.Should().ThrowAsync<SqlException>("primary key constraint must prevent duplicate event processing");
    }
}
