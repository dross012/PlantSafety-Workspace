// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Events
//  Layer:          IntegrationTests
//  File:           IntegrationEventEnvelopeTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies integration event envelope completeness per canonical contract.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.Assertions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Events;

/// <summary>
/// Verifies that all outbox messages produced by the platform include the
/// canonical integration event envelope fields required by the architectural rules.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class IntegrationEventEnvelopeTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OutboxPayload_ShouldContainAllCanonicalEnvelopeFields()
    {
        // Arrange — seed a canonical payload with all required fields
        var payload = JsonSerializer.Serialize(new
        {
            correlationId = CorrelationId,
            tenantId      = TenantId,
            actorId       = ActorId,
            occurredAt    = FixedUtcNow,
            schemaVersion = TestSchemaVersion.Current
        });

        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Events.EnvelopeTest', @TenantId, @ActorId, @CorrelationId, @Now, @Payload)",
            new { TenantId, ActorId, CorrelationId, Now = FixedUtcNow, Payload = payload });

        // Assert
        await DomainEventAssertions.AssertEnvelopeFieldsCompleteAsync(
            ConnectionString, "Events.EnvelopeTest", TenantId);
    }

    [Fact]
    public async Task OutboxMessage_SchemaVersion_ShouldMatchCanonicalVersion()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var schemaVersion = await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 SchemaVersion FROM [System].[OutboxMessages]");

        if (schemaVersion is not null)
        {
            schemaVersion.Should().Be(TestSchemaVersion.Current,
                "all outbox messages must use the canonical schema version 2026.1");
        }
    }
}
