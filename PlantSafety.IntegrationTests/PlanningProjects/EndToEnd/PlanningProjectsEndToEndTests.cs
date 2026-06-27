// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PlanningProjects
//  Layer:          IntegrationTests
//  File:           PlanningProjectsEndToEndTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    End-to-end integration tests for the PlanningProjects workflow.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PlanningProjects.EndToEnd;

/// <summary>End-to-end tests for the PlanningProjects module.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class PlanningProjectsEndToEndTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PlanningProject_ShouldEnqueueOutboxEvent_WhenCreated()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), 'PlanningProjects.ProjectCreated', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(
            ConnectionString, "PlanningProjects.ProjectCreated", TenantId);
    }
}
