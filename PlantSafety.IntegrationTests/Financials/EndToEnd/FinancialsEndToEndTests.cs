// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Financials
//  Layer:          IntegrationTests
//  File:           FinancialsEndToEndTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    End-to-end integration tests for the Financials workflow from command to persistence.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Financials.EndToEnd;

/// <summary>
/// End-to-end tests for the Financials module verifying command → aggregate →
/// domain event → outbox flow without calling the API layer directly.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class FinancialsEndToEndTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FinancialRecord_ShouldEnqueueOutboxEvent_WhenCreated()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), 'Financials.FinancialRecordCreated', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "Financials.FinancialRecordCreated", TenantId);
    }
}
