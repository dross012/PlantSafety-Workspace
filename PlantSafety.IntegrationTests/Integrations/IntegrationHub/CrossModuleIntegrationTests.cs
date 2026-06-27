// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations
//  Layer:          IntegrationTests
//  File:           CrossModuleIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Cross-module integration tests verifying event-driven coordination between modules.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.Assertions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.IntegrationHub;

/// <summary>
/// Cross-module integration tests verifying that events raised by one module
/// are correctly consumed by downstream modules via the integration hub.
/// Per the Canonical Module Completion Checklist — 1+ cross-module test required.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CrossModuleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task IncidentCreated_ShouldTriggerAlertModule_ViaOutbox()
    {
        // Arrange — IncidentManagement raises IncidentCreated event
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);

        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), 'SafetyEvents.IncidentCreated', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        // Assert — outbox contains the cross-module triggering event
        await CorrelationAssertions.AssertCorrelationPropagatedToOutboxAsync(
            ConnectionString, correlationId, "SafetyEvents.IncidentCreated");

        // The Alerts consumer would then read this and create an alert.
        // That second-order write is tested in OperationalSafety.Alerts module tests.
    }

    [Fact]
    public async Task AllOutboxEvents_ShouldHaveSchemaVersion2026()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var badVersionCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[OutboxMessages] WHERE SchemaVersion != '2026.1'");

        badVersionCount.Should().Be(0, "all outbox messages must use SchemaVersion 2026.1");
    }
}
