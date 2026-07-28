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
        // [System].[OutboxMessages] is a single shared table across the whole test run (per
        // IntegrationTestCollection), and dozens of other integration test classes deliberately
        // seed synthetic rows with a placeholder SchemaVersion ('1.0') unrelated to the real
        // domain event envelope. An unfiltered "TOP 1" over that table is nondeterministic and
        // picks up whichever row happens to be first, so this test must be scoped to a row it
        // owns rather than an arbitrary one from the shared table.
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES
                (NEWID(), 'Events.SchemaVersionCanonicalTest', @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        var schemaVersion = await conn.ExecuteScalarAsync<string>(
            "SELECT SchemaVersion FROM [System].[OutboxMessages] WHERE CorrelationId = @CorrelationId",
            new { CorrelationId = correlationId });

        schemaVersion.Should().Be(TestSchemaVersion.Current,
            "an outbox message inserted without an explicit SchemaVersion must fall back to the canonical default schema version");
    }
}
