// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Queries
//  Layer:          IntegrationTests
//  File:           QueryDeterminismTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies that repeated query executions return identical, deterministic results.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Queries;

/// <summary>
/// Verifies that query operations are deterministic: repeated calls with the same
/// parameters return identical results in the same order.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class QueryDeterminismTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Query_OutboxMessages_ShouldReturnDeterministicOrder()
    {
        // Arrange — seed two messages with known timestamps
        await using var conn = new SqlConnection(ConnectionString);
        var t1 = FixedUtcNow.AddSeconds(-10);
        var t2 = FixedUtcNow;

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Queries.OrderTest.A', @TenantId, @ActorId, @CorrelationId, @T1, '{}'),
                (NEWID(), 'Queries.OrderTest.B', @TenantId, @ActorId, @CorrelationId, @T2, '{}')",
            new { TenantId, ActorId, CorrelationId, T1 = t1, T2 = t2 });

        // Act — query twice with explicit ORDER BY
        var firstCall = (await conn.QueryAsync<string>(
            @"SELECT TOP 2 EventType FROM [System].[OutboxMessages]
              WHERE TenantId = @TenantId AND EventType LIKE 'Queries.OrderTest%'
              ORDER BY OccurredAt ASC",
            new { TenantId })).ToList();

        var secondCall = (await conn.QueryAsync<string>(
            @"SELECT TOP 2 EventType FROM [System].[OutboxMessages]
              WHERE TenantId = @TenantId AND EventType LIKE 'Queries.OrderTest%'
              ORDER BY OccurredAt ASC",
            new { TenantId })).ToList();

        // Assert — same order both calls
        firstCall.Should().BeEquivalentTo(secondCall, opts => opts.WithStrictOrdering(),
            "queries with explicit ORDER BY must return deterministic results");
        firstCall[0].Should().Contain("OrderTest.A");
    }
}
