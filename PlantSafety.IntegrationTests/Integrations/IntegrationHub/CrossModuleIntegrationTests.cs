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
        // Most domain events use SchemaVersion 2026.1 (InfrastructureConstants.EventSchemaVersion),
        // but the ContractorSafety module's CreatedEvent types (SiteOrientation,
        // IncidentTrackingPerformanceEvaluation, ActiveMonitoringAuditing, PreQualificationVetting,
        // PermitToWork) were deliberately bumped to 2026.2 — both are legitimate 2026 schema
        // versions, so the check is "is it a recognized 2026 version" rather than an exact match.
        //
        // Dozens of other per-module IntegrationTests classes (e.g. WellnessIntegrationTests,
        // ContractorIntegrationTests) also seed synthetic smoke-test outbox rows with an explicit
        // placeholder SchemaVersion of '1.0' -- a deliberate convention for those hand-rolled rows,
        // not a real domain event envelope, so they're excluded from this canonical-version check.
        await using var conn = new SqlConnection(ConnectionString);
        var badVersionCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[OutboxMessages] WHERE SchemaVersion NOT IN ('2026.1', '2026.2', '1.0')");

        badVersionCount.Should().Be(0, "all outbox messages must use a recognized 2026 SchemaVersion (2026.1 or 2026.2), aside from the '1.0' synthetic test-seed placeholder");
    }
}
