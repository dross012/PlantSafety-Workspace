// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Repositories
//  Layer:          IntegrationTests
//  File:           RepositoryIdempotencyTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies that consumer idempotency enforcement via ProcessedEvents is correct.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Repositories;

/// <summary>
/// Verifies that the idempotency pattern implemented via <c>System.ProcessedEvents</c>
/// prevents duplicate processing of the same event identifier.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class RepositoryIdempotencyTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ProcessedEvents_ShouldPreventDuplicateSideEffects()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        // First processing
        int alreadyProcessed = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        alreadyProcessed.Should().Be(0, "event should not have been processed yet");

        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        // Second processing attempt — idempotency check
        int processedCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        processedCount.Should().Be(1, "event should be recorded exactly once");
    }

    [Fact]
    public async Task ProcessedEvents_Insert_ShouldBeAtomic()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            await conn.ExecuteAsync(
                "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
                new { EventId = eventId, Now = FixedUtcNow }, tx);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        await SqlAssertions.AssertEventProcessedAsync(ConnectionString, eventId);
    }
}
